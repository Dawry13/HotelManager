-- =====================================================
-- extension para los modulos de bloques y tipos de habitacion
-- ejecutar despues de 01_HotelManagerDB.sql
-- se puede ejecutar mas de una vez
-- =====================================================

use HotelManagerDB;
go

-- bloque (torre / edificio dentro de un hotel)
if object_id('dbo.bloques') is null
create table bloques (
    id_bloque int identity(1,1) not null,
    id_hotel int not null,
    codigo varchar(10) not null,
    nombre varchar(100) not null,
    descripcion varchar(250) null,
    estado varchar(20) not null constraint df_bloques_estado default 'activo',
    constraint pk_bloques primary key (id_bloque),
    constraint fk_bloques_hoteles foreign key (id_hotel) references hoteles(id_hotel),
    constraint ck_bloques_estado check (estado in ('activo','inactivo')),
    constraint uq_bloques_hotel_codigo unique (id_hotel, codigo)
);
go

-- tipo de habitacion (catalogo)
if object_id('dbo.tipos_habitacion') is null
create table tipos_habitacion (
    id_tipo_habitacion int identity(1,1) not null,
    nombre varchar(50) not null,
    descripcion varchar(250) null,
    capacidad_maxima int not null,
    tarifa_base decimal(10,2) not null constraint df_tipos_habitacion_tarifa default 0,
    estado varchar(20) not null constraint df_tipos_habitacion_estado default 'activo',
    constraint pk_tipos_habitacion primary key (id_tipo_habitacion),
    constraint uq_tipos_habitacion_nombre unique (nombre),
    constraint ck_tipos_habitacion_estado check (estado in ('activo','inactivo')),
    constraint ck_tipos_habitacion_capacidad check (capacidad_maxima > 0),
    constraint ck_tipos_habitacion_tarifa check (tarifa_base >= 0)
);
go

-- un piso puede pertenecer a un bloque (opcional)
if col_length('dbo.pisos', 'id_bloque') is null
begin
    alter table pisos add id_bloque int null;
    alter table pisos add constraint fk_pisos_bloques foreign key (id_bloque) references bloques(id_bloque);
end
go

-- una habitacion tiene un tipo (opcional)
if col_length('dbo.habitaciones', 'id_tipo_habitacion') is null
begin
    alter table habitaciones add id_tipo_habitacion int null;
    alter table habitaciones add constraint fk_habitaciones_tipos foreign key (id_tipo_habitacion) references tipos_habitacion(id_tipo_habitacion);
end
go

-- =====================================================
-- datos de prueba
-- =====================================================

if not exists (select 1 from tipos_habitacion)
insert into tipos_habitacion (nombre, descripcion, capacidad_maxima, tarifa_base) values
('sencilla', 'una cama individual', 1, 2500.00),
('doble', 'dos camas o una cama matrimonial', 2, 3500.00),
('triple', 'tres camas individuales', 3, 4500.00),
('familiar', 'habitacion amplia para familias', 4, 5500.00);
go

if not exists (select 1 from bloques)
begin
    insert into bloques (id_hotel, codigo, nombre, descripcion) values
    (1, 'A', 'torre a', 'torre principal'),
    (2, 'V', 'villas', 'villas frente al mar');

    update pisos set id_bloque = 1 where id_hotel = 1 and id_bloque is null;
    update pisos set id_bloque = 2 where id_hotel = 2 and id_bloque is null;

    update habitaciones set id_tipo_habitacion = 2 where capacidad = 2 and id_tipo_habitacion is null;
    update habitaciones set id_tipo_habitacion = 3 where capacidad = 3 and id_tipo_habitacion is null;
    update habitaciones set id_tipo_habitacion = 4 where capacidad = 4 and id_tipo_habitacion is null;
end
go
