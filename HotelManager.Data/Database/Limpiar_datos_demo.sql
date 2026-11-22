-- =========================================================
-- HotelManagerDB - Limpieza de datos (ejecucion manual, una vez)
--
-- Deja la base con:
--   * 3 hoteles: uno de 3, uno de 4 y uno de 5 estrellas
--     (se conservan los 3 hoteles mas recientes, en orden: 3, 4 y 5 estrellas).
--   * Cada hotel con 1 bloque, 1 piso y 1 habitacion.
--   * 1 solo huesped (el primero registrado), sin cuenta de usuario.
--   * Usuarios: solo administradores y recepcionistas.
--   * Sin reservas, notificaciones, historial ni tarifas por temporada.
--   * Los tipos de habitacion se conservan (son el catalogo).
--
-- Todo corre en una transaccion: si algo falla no se borra nada.
-- Como ejecutarlo: Visual Studio > Ver > Explorador de objetos de SQL Server >
--   (localdb)\MSSQLLocalDB > Bases de datos > HotelManagerDB > clic derecho
--   "Nueva consulta" > pegar este archivo > Ejecutar (Ctrl+Shift+E).
-- =========================================================
use HotelManagerDB;
set nocount on;
set xact_abort on;
set quoted_identifier on;   -- requerido por los indices filtrados
set ansi_nulls on;

begin transaction;

-- ---------- 1. Hoteles a conservar (los 3 mas recientes; se crean si faltan) ----------
declare @hoteles table (orden int identity(1,1), id_hotel int);
insert into @hoteles (id_hotel)
select id_hotel from (select top 3 id_hotel from hoteles order by id_hotel desc) x order by id_hotel;

declare @n int;
while (select count(*) from @hoteles) < 3
begin
    set @n = (select count(*) from @hoteles) + 1;
    insert into hoteles (nombre, direccion, estado, categoria)
    values ('hotel ' + cast(@n + 2 as varchar(2)) + ' estrellas', 'direccion por definir', 'activo', @n + 2);
    insert into @hoteles (id_hotel) values (cast(scope_identity() as int));
end

-- orden 1 -> 3 estrellas, 2 -> 4 estrellas, 3 -> 5 estrellas
update ho set categoria = h.orden + 2, estado = 'activo'
from hoteles ho join @hoteles h on h.id_hotel = ho.id_hotel;

-- ---------- 2. Un bloque, un piso y una habitacion por hotel ----------
declare @conservar table (id_hotel int, id_bloque int, id_piso int, id_habitacion int);
insert into @conservar (id_hotel) select id_hotel from @hoteles;

-- Bloque: el primero del hotel, o uno nuevo
update c set id_bloque = (select min(b.id_bloque) from bloques b where b.id_hotel = c.id_hotel) from @conservar c;
insert into bloques (id_hotel, codigo, nombre, descripcion, estado)
select c.id_hotel, 'A', 'torre a', null, 'activo' from @conservar c where c.id_bloque is null;
update c set id_bloque = (select min(b.id_bloque) from bloques b where b.id_hotel = c.id_hotel) from @conservar c where c.id_bloque is null;
update b set estado = 'activo' from bloques b join @conservar c on c.id_bloque = b.id_bloque;

-- Piso: el que ya tenga habitaciones (si hay), si no el primero, o uno nuevo
update c set id_piso = coalesce(
        (select min(p.id_piso) from pisos p where p.id_hotel = c.id_hotel
            and exists (select 1 from habitaciones h where h.id_piso = p.id_piso)),
        (select min(p.id_piso) from pisos p where p.id_hotel = c.id_hotel))
from @conservar c;
insert into pisos (numero, id_hotel, id_bloque, estado)
select 1, c.id_hotel, c.id_bloque, 'activo' from @conservar c where c.id_piso is null;
update c set id_piso = (select min(p.id_piso) from pisos p where p.id_hotel = c.id_hotel) from @conservar c where c.id_piso is null;
update p set id_bloque = c.id_bloque, estado = 'activo' from pisos p join @conservar c on c.id_piso = p.id_piso;

-- Habitacion: la primera del piso elegido, o una nueva sin precio (tarifa 0 = la asigna usted)
update c set id_habitacion = (select min(h.id_habitacion) from habitaciones h where h.id_piso = c.id_piso) from @conservar c;
insert into habitaciones (numero, capacidad, tarifa, estado, id_piso, id_hotel, id_tipo_habitacion)
select '101', 2, 0, 'disponible', c.id_piso, c.id_hotel,
       (select min(t.id_tipo_habitacion) from tipos_habitacion t where t.capacidad_maxima >= 2)
from @conservar c where c.id_habitacion is null;
update c set id_habitacion = (select min(h.id_habitacion) from habitaciones h where h.id_piso = c.id_piso) from @conservar c where c.id_habitacion is null;
update h set estado = 'disponible' from habitaciones h join @conservar c on c.id_habitacion = h.id_habitacion;

-- ---------- 3. Movimientos: reservas y todo lo que depende de ellas ----------
delete from notificaciones;
delete from historial_reservas;
delete from reservas;
delete from tarifas;

-- ---------- 4. Usuarios: solo administrador y recepcionista ----------
delete from usuarios where rol not in ('administrador', 'recepcionista');
update usuarios set id_huesped = null where id_huesped is not null;

-- ---------- 5. Huespedes: solo el primero ----------
declare @huesped int = (select min(id_huesped) from huespedes);
delete from huespedes where id_huesped <> @huesped;

-- ---------- 6. Estructura sobrante ----------
delete from habitaciones where id_habitacion not in (select id_habitacion from @conservar);
delete from pisos where id_piso not in (select id_piso from @conservar);
delete from bloques where id_bloque not in (select id_bloque from @conservar);
delete from hoteles where id_hotel not in (select id_hotel from @conservar);

commit transaction;

-- ---------- Resultado ----------
select ho.id_hotel, ho.nombre, ho.categoria as estrellas, b.nombre as bloque, p.numero as piso,
       h.numero as habitacion, h.tarifa
from hoteles ho
join bloques b on b.id_hotel = ho.id_hotel
join pisos p on p.id_hotel = ho.id_hotel
join habitaciones h on h.id_piso = p.id_piso
order by ho.categoria;

select id_huesped, nombre, documento, correo from huespedes;
select nombre_usuario, rol, estado from usuarios order by rol;
