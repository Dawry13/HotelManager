# Scripts de la base de datos `HotelManagerDB`

Hay **una sola base de datos**. Estos scripts la construyen por etapas y la aplicación
los ejecuta sola al arrancar (`InicializadorBaseDatos`), aplicando únicamente los que falten.

| Script | Qué hace |
|---|---|
| `01_HotelManagerDB.sql` | Crea la base y las tablas principales: hoteles, pisos, habitaciones, huéspedes, reservas, tarifas, notificaciones e historial. |
| `02_bloques_tipos_habitacion.sql` | Agrega bloques y tipos de habitación. |
| `03_usuarios_roles.sql` | Agrega usuarios y roles para el inicio de sesión. |
| `04_categorias_caracteristicas.sql` | Agrega la categoría del hotel (3, 4 o 5 estrellas) y las características de hoteles y habitaciones. |

Los scripts están incrustados en `HotelManager.Data.dll`. Si agrega uno nuevo, regístrelo en
`HotelManager.Data.csproj` como `EmbeddedResource` y llámelo desde `InicializadorBaseDatos`.
