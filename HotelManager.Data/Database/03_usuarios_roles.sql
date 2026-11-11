
use HotelManagerDB;
go

if object_id('dbo.usuarios') is null
create table usuarios (
    id_usuario int identity(1,1) not null,
    nombre_usuario varchar(50) not null,
    nombre varchar(150) not null,
    correo varchar(150) null,
    clave_hash varchar(300) not null,
    rol varchar(30) not null,
    estado varchar(20) not null constraint df_usuarios_estado default 'activo',
    debe_cambiar_clave bit not null constraint df_usuarios_cambiar_clave default 0,
    id_huesped int null,
    fecha_creacion datetime2 not null constraint df_usuarios_fecha_creacion default sysdatetime(),
    ultimo_acceso datetime2 null,
    constraint pk_usuarios primary key (id_usuario),
    constraint uq_usuarios_nombre_usuario unique (nombre_usuario),
    constraint ck_usuarios_rol check (rol in ('administrador','recepcionista','huesped','servicio_notificacion')),
    constraint ck_usuarios_estado check (estado in ('activo','inactivo')),
    constraint fk_usuarios_huespedes foreign key (id_huesped) references huespedes(id_huesped),
    -- un usuario con rol huesped debe estar vinculado a su registro de huesped
    constraint ck_usuarios_rol_huesped check (rol <> 'huesped' or id_huesped is not null)
);
go

-- un huesped solo puede tener una cuenta de usuario
if not exists (select 1 from sys.indexes where name = 'ux_usuarios_id_huesped')
    create unique index ux_usuarios_id_huesped on usuarios (id_huesped) where id_huesped is not null;
go
