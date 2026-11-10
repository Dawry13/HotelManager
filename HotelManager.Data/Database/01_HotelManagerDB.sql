if db_id('hotelmanager') is null
    create database HotelManagerDB;
go

use  HotelManagerDB;
go

-- =====================================================
-- tablas
-- =====================================================

-- hotel
create table hoteles (
    id_hotel int identity(1,1) not null,
    nombre varchar(150) not null,
    direccion varchar(250) not null,
    estado varchar(20)  not null constraint df_hoteles_estado default 'activo',
    constraint pk_hoteles primary key (id_hotel),
    constraint ck_hoteles_estado check (estado in ('activo','inactivo'))
);
go

-- piso
create table pisos (
    id_piso int identity(1,1) not null,
    numero int not null,
    estado varchar(20) not null constraint df_pisos_estado default 'activo',
    id_hotel int not null,
    constraint pk_pisos primary key (id_piso),
    constraint fk_pisos_hoteles foreign key (id_hotel) references hoteles(id_hotel),
    constraint ck_pisos_estado check (estado in ('activo','inactivo')),
    constraint ck_pisos_numero check (numero >= 0),
    -- permite la fk compuesta desde habitaciones (para unicidad por hotel)
    constraint uq_pisos_piso_hotel unique (id_piso, id_hotel)
);
go

-- rn-003: no pueden existir dos pisos activos con el mismo numero en un hotel
create unique index ux_pisos_hotel_numero_activo
    on pisos (id_hotel, numero)
    where estado = 'activo';
go

-- habitacion
create table habitaciones (
    id_habitacion int identity(1,1) not null,
    numero varchar(10) not null,
    capacidad int not null,
    tarifa decimal(10,2) not null,
    estado varchar(20) not null constraint df_habitaciones_estado default 'disponible',
    id_piso int not null,
    id_hotel int not null,
    constraint pk_habitaciones primary key (id_habitacion),
    constraint fk_habitaciones_pisos foreign key (id_piso, id_hotel) references pisos(id_piso, id_hotel),
    constraint ck_habitaciones_estado check (estado in ('disponible','mantenimiento','fuera_de_servicio','inactiva')),
    constraint ck_habitaciones_capacidad check (capacidad > 0),
    constraint ck_habitaciones_tarifa check (tarifa >= 0),
    -- rn-005: numero unico dentro del hotel
    constraint uq_habitaciones_hotel_numero unique (id_hotel, numero)
);
go

-- huesped
create table huespedes (
    id_huesped int identity(1,1) not null,
    nombre varchar(150) not null,
    documento varchar(30)  not null,
    telefono varchar(20)  null,
    correo varchar(150) null,
    constraint pk_huespedes primary key (id_huesped),
    constraint uq_huespedes_documento unique (documento)
);
go

-- tarifa (valor aplicable segun vigencia)
create table tarifas (
    id_tarifa int identity(1,1) not null,
    id_habitacion int not null,
    valor_noche decimal(10,2) not null,
    vigencia_desde date not null,
    vigencia_hasta date null,
    constraint pk_tarifas primary key (id_tarifa),
    constraint fk_tarifas_habitaciones foreign key (id_habitacion) references habitaciones(id_habitacion),
    constraint ck_tarifas_valor check (valor_noche >= 0),
    constraint ck_tarifas_vigencia check (vigencia_hasta is null or vigencia_hasta >= vigencia_desde)
);
go

-- reserva
create table reservas (
    id_reserva int identity(1,1) not null,
    fecha_entrada date not null,
    fecha_salida date not null,
    cantidad_huespedes int not null,
    costo_total decimal(12,2) not null constraint df_reservas_costo default 0,
    estado varchar(20) not null constraint df_reservas_estado default 'pendiente',
    id_habitacion int not null,
    id_huesped int not null,
    fecha_registro datetime2 not null constraint df_reservas_fecha_registro default sysdatetime(),
    constraint pk_reservas primary key (id_reserva),
    constraint fk_reservas_habitaciones foreign key (id_habitacion) references habitaciones(id_habitacion),
    constraint fk_reservas_huespedes foreign key (id_huesped) references huespedes(id_huesped),
    constraint ck_reservas_estado check (estado in ('pendiente','confirmada','modificada','cancelada','finalizada')),
    -- rn-008: la salida debe ser posterior a la entrada
    constraint ck_reservas_fechas check (fecha_salida > fecha_entrada),
    constraint ck_reservas_cantidad check (cantidad_huespedes > 0),
    constraint ck_reservas_costo check (costo_total >= 0)
);
go

create index ix_reservas_habitacion_fechas on reservas (id_habitacion, fecha_entrada, fecha_salida) include (estado);
create index ix_reservas_huesped on reservas (id_huesped);
go

-- notificacion (rn-014, rn-015)
create table notificaciones (
    id_notificacion int identity(1,1) not null,
    tipo varchar(20) not null,
    fecha datetime2 not null constraint df_notificaciones_fecha default sysdatetime(),
    estado varchar(20) not null constraint df_notificaciones_estado default 'pendiente',
    id_reserva int not null,
    detalle_error varchar(500) null,
    constraint pk_notificaciones primary key (id_notificacion),
    constraint fk_notificaciones_reservas foreign key (id_reserva) references reservas(id_reserva),
    constraint ck_notificaciones_tipo check (tipo in ('confirmacion','modificacion','cancelacion')),
    constraint ck_notificaciones_estado check (estado in ('pendiente','enviada','fallida'))
);
go

create index ix_notificaciones_reserva on notificaciones (id_reserva);
go

-- historial de reserva (auditoria)
create table historial_reservas (
    id_historial int identity(1,1) not null,
    fecha datetime2 not null constraint df_historial_fecha default sysdatetime(),
    accion varchar(50) not null,
    estado_anterior varchar(20) null,
    estado_nuevo varchar(20) null,
    id_reserva int not null,
    constraint pk_historial_reservas primary key (id_historial),
    constraint fk_historial_reservas foreign key (id_reserva) references reservas(id_reserva)
);
go

create index ix_historial_reserva on historial_reservas (id_reserva);
go

-- =====================================================
-- funciones
-- =====================================================

-- rn-013: costo total = noches * tarifa aplicable
-- usa la tarifa vigente a la fecha de entrada; si no existe, la tarifa base de la habitacion
create function fn_calcular_costo (
    @id_habitacion int,
    @fecha_entrada date,
    @fecha_salida  date
)
returns decimal(12,2)
as
begin
    declare @noches int = datediff(day, @fecha_entrada, @fecha_salida);
    declare @valor decimal(10,2);

    select top 1 @valor = t.valor_noche
    from tarifas t
    where t.id_habitacion = @id_habitacion
      and t.vigencia_desde <= @fecha_entrada
      and (t.vigencia_hasta is null or t.vigencia_hasta >= @fecha_entrada)
    order by t.vigencia_desde desc;

    if @valor is null
        select @valor = tarifa from habitaciones where id_habitacion = @id_habitacion;

    return @noches * isnull(@valor, 0);
end
go

-- =====================================================
-- triggers (reglas de negocio)
-- =====================================================

-- rn-006: un piso inactivo no puede recibir nuevas habitaciones
create trigger tr_habitaciones_piso_activo
on habitaciones
after insert
as
begin
    set nocount on;
    if exists (
        select 1
        from inserted i
        join pisos p on p.id_piso = i.id_piso
        where p.estado <> 'activo'
    )
    begin
        raiserror('un piso inactivo no puede recibir nuevas habitaciones.', 16, 1);
        rollback transaction;
        return;
    end
end
go

-- rn-009: no se permiten reservas con fecha de entrada anterior a la actual
create trigger tr_reservas_fecha_actual
on reservas
after insert
as
begin
    set nocount on;
    if exists (select 1 from inserted where fecha_entrada < cast(getdate() as date))
    begin
        raiserror('no se permiten reservas para fechas anteriores a la fecha actual.', 16, 1);
        rollback transaction;
        return;
    end
end
go

-- rn-007, rn-010, rn-011: habitacion reservable, sin solapamiento y dentro de la capacidad
create trigger tr_reservas_validaciones
on reservas
after insert, update
as
begin
    set nocount on;

    -- solo se validan reservas activas (una cancelada no bloquea nada: rn-012)
    -- rn-007: habitacion fuera de servicio, en mantenimiento o inactiva no se reserva
    if exists (
        select 1
        from inserted i
        join habitaciones h on h.id_habitacion = i.id_habitacion
        join pisos p on p.id_piso = h.id_piso
        join hoteles ho on ho.id_hotel = p.id_hotel
        where i.estado in ('pendiente','confirmada','modificada')
          and (h.estado <> 'disponible' or p.estado <> 'activo' or ho.estado <> 'activo')
    )
    begin
        raiserror('la habitacion no es reservable (fuera de servicio, en mantenimiento o inactiva).', 16, 1);
        rollback transaction;
        return;
    end

    -- rn-011: la cantidad de huespedes no puede superar la capacidad
    if exists (
        select 1
        from inserted i
        join habitaciones h on h.id_habitacion = i.id_habitacion
        where i.estado in ('pendiente','confirmada','modificada')
          and i.cantidad_huespedes > h.capacidad
    )
    begin
        raiserror('la cantidad de huespedes supera la capacidad de la habitacion.', 16, 1);
        rollback transaction;
        return;
    end

    -- rn-010: no pueden existir reservas activas solapadas para una misma habitacion
    if exists (
        select 1
        from inserted i
        join reservas r
          on r.id_habitacion = i.id_habitacion
         and r.id_reserva <> i.id_reserva
         and r.estado in ('pendiente','confirmada','modificada')
         and i.fecha_entrada < r.fecha_salida
         and i.fecha_salida  > r.fecha_entrada
        where i.estado in ('pendiente','confirmada','modificada')
    )
    begin
        raiserror('ya existe una reserva activa que se solapa para esta habitacion.', 16, 1);
        rollback transaction;
        return;
    end
end
go

-- auditoria: registra automaticamente los cambios de las reservas
create trigger tr_reservas_historial
on reservas
after insert, update
as
begin
    set nocount on;

    -- altas
    insert into historial_reservas (accion, estado_anterior, estado_nuevo, id_reserva)
    select 'creacion', null, i.estado, i.id_reserva
    from inserted i
    where not exists (select 1 from deleted d where d.id_reserva = i.id_reserva);

    -- modificaciones
    insert into historial_reservas (accion, estado_anterior, estado_nuevo, id_reserva)
    select
        case when i.estado = 'cancelada' and d.estado <> 'cancelada' then 'cancelacion'
             when i.estado <> d.estado then 'cambio_estado'
             else 'modificacion' end,
        d.estado, i.estado, i.id_reserva
    from inserted i
    join deleted d on d.id_reserva = i.id_reserva;
end
go

-- =====================================================
-- procedimientos almacenados
-- =====================================================

-- cu-006: consultar disponibilidad por fechas, capacidad y hotel (opcional)
create procedure sp_consultar_disponibilidad
    @fecha_entrada date,
    @fecha_salida  date,
    @cantidad_huespedes int,
    @id_hotel int = null
as
begin
    set nocount on;

    if @fecha_salida <= @fecha_entrada
    begin
        raiserror('la fecha de salida debe ser posterior a la fecha de entrada.', 16, 1);
        return;
    end

    if @fecha_entrada < cast(getdate() as date)
    begin
        raiserror('no se permiten consultas para fechas anteriores a la fecha actual.', 16, 1);
        return;
    end

    select
        ho.id_hotel,
        ho.nombre as hotel,
        p.numero as piso,
        h.id_habitacion,
        h.numero as habitacion,
        h.capacidad,
        h.tarifa,
        dbo.fn_calcular_costo(h.id_habitacion, @fecha_entrada, @fecha_salida) as costo_estimado
    from habitaciones h
    join pisos p on p.id_piso = h.id_piso
    join hoteles ho on ho.id_hotel = p.id_hotel
    where h.estado = 'disponible'
      and p.estado = 'activo'
      and ho.estado = 'activo'
      and h.capacidad >= @cantidad_huespedes
      and (@id_hotel is null or ho.id_hotel = @id_hotel)
      and not exists (
            select 1
            from reservas r
            where r.id_habitacion = h.id_habitacion
              and r.estado in ('pendiente','confirmada','modificada')
              and @fecha_entrada < r.fecha_salida
              and @fecha_salida  > r.fecha_entrada
      )
    order by ho.nombre, p.numero, h.numero;
end
go

-- cu-007: crear reserva (calcula el costo y genera la notificacion)
create procedure sp_crear_reserva
    @id_huesped int,
    @id_habitacion int,
    @fecha_entrada date,
    @fecha_salida date,
    @cantidad_huespedes int,
    @id_reserva int output
as
begin
    set nocount on;
    set xact_abort on;

    begin transaction;

    declare @costo decimal(12,2) = dbo.fn_calcular_costo(@id_habitacion, @fecha_entrada, @fecha_salida);

    insert into reservas (fecha_entrada, fecha_salida, cantidad_huespedes, costo_total, estado, id_habitacion, id_huesped)
    values (@fecha_entrada, @fecha_salida, @cantidad_huespedes, @costo, 'confirmada', @id_habitacion, @id_huesped);

    set @id_reserva = scope_identity();

    insert into notificaciones (tipo, estado, id_reserva)
    values ('confirmacion', 'pendiente', @id_reserva);

    commit transaction;
end
go

-- cu-009: cancelar reserva
create procedure sp_cancelar_reserva
    @id_reserva int
as
begin
    set nocount on;
    set xact_abort on;

    if not exists (select 1 from reservas where id_reserva = @id_reserva)
    begin
        raiserror('la reserva no existe.', 16, 1);
        return;
    end

    if exists (select 1 from reservas where id_reserva = @id_reserva and estado = 'cancelada')
    begin
        raiserror('la reserva ya esta cancelada.', 16, 1);
        return;
    end

    begin transaction;

    update reservas set estado = 'cancelada' where id_reserva = @id_reserva;

    insert into notificaciones (tipo, estado, id_reserva)
    values ('cancelacion', 'pendiente', @id_reserva);

    commit transaction;
end
go

-- =====================================================
-- vistas
-- =====================================================

-- cu-010: historial de reservas
create view vw_historial_reservas
as
select
    h.id_historial,
    h.fecha,
    h.accion,
    h.estado_anterior,
    h.estado_nuevo,
    r.id_reserva,
    hu.nombre as huesped,
    ha.numero as habitacion,
    ho.nombre as hotel
from historial_reservas h
join reservas r on r.id_reserva = h.id_reserva
join huespedes hu on hu.id_huesped = r.id_huesped
join habitaciones ha on ha.id_habitacion = r.id_habitacion
join hoteles ho on ho.id_hotel = ha.id_hotel;
go

-- =====================================================
-- datos de prueba
-- =====================================================

insert into hoteles (nombre, direccion) values
('hotel caribe santo domingo', 'av. george washington 100, santo domingo'),
('hotel playa punta cana', 'bavaro, punta cana, la altagracia');

insert into pisos (numero, id_hotel) values (1,1),(2,1),(1,2);

insert into habitaciones (numero, capacidad, tarifa, id_piso, id_hotel) values
('101', 2, 3500.00, 1, 1),
('102', 4, 5500.00, 1, 1),
('201', 2, 4000.00, 2, 1),
('101', 3, 6000.00, 3, 2);

insert into huespedes (nombre, documento, telefono, correo) values
('juan perez', '001-0000000-1', '809-555-0101', 'juan@correo.com'),
('maria gomez', '001-0000000-2', '829-555-0202', 'maria@correo.com');
go

-- ejemplo de uso:
-- exec sp_consultar_disponibilidad @fecha_entrada = '2026-12-01', @fecha_salida = '2026-12-05', @cantidad_huespedes = 2;
-- declare @id int;
-- exec sp_crear_reserva 1, 1, '2026-12-01', '2026-12-05', 2, @id output;
-- exec sp_cancelar_reserva @id;
-- select * from vw_historial_reservas;