-- =========================================================
-- HotelManagerDB - Extension 04
-- Categoria del hotel (3, 4 o 5 estrellas) y caracteristicas
-- (amenidades) de hoteles y habitaciones.
-- Las caracteristicas se guardan como texto separado por ';'
-- (ej.: 'Wi-Fi;Piscina;Spa').
-- Se aplica automaticamente desde InicializadorBaseDatos.
-- =========================================================

if col_length('dbo.hoteles', 'categoria') is null
    alter table hoteles add categoria int not null constraint df_hoteles_categoria default 3;
go

if object_id('ck_hoteles_categoria', 'C') is null
    alter table hoteles add constraint ck_hoteles_categoria check (categoria between 3 and 5);
go

if col_length('dbo.hoteles', 'caracteristicas') is null
    alter table hoteles add caracteristicas varchar(500) null;
go

if col_length('dbo.hoteles', 'descripcion') is null
    alter table hoteles add descripcion varchar(500) null;
go

if col_length('dbo.habitaciones', 'caracteristicas') is null
    alter table habitaciones add caracteristicas varchar(500) null;
go
