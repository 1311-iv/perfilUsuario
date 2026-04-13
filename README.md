# Sistema de Gestión de Perfiles de Usuario

## Descripción
Sistema web para la gestión de perfiles de usuario. Backend API REST con ASP.NET Web API 2 (.NET Framework 4.8) + Dapper. Frontend con HTML/CSS/JavaScript vanilla.

## Estructura del Proyecto

```
perfilUsuario/
├── PerfilUsuarioAPI/          # Backend - Código fuente
│   ├── App_Start/
│   │   └── WebApiConfig.cs    # CORS + Rutas + JSON config
│   ├── Controllers/
│   │   ├── AuthController.cs
│   │   ├── DireccionesController.cs
│   │   ├── PerfilUsuarioController.cs
│   │   ├── RolesController.cs
│   │   └── UsuariosController.cs
│   ├── Models/
│   │   ├── Direccion.cs
│   │   ├── LoginRequest.cs
│   │   ├── PerfilUsuario.cs
│   │   ├── PerfilUsuarioRequest.cs
│   │   ├── Rol.cs
│   │   ├── Telefono.cs
│   │   ├── Usuario.cs
│   │   └── UsuarioRequest.cs
│   ├── Global.asax
│   ├── Global.asax.cs
│   ├── Web.config              # Connection string aquí
│   ├── PerfilUsuarioAPI.csproj
│   └── packages.config
├── PerfilUsuarioAPI.sln         # Abrir con VS 2022
├── Frontend/
│   ├── css/styles.css
│   ├── js/config.js            # URL de la API aquí
│   ├── login.html
│   ├── perfiles.html
│   ├── perfil-form.html
│   ├── perfil-detalle.html
│   ├── direcciones.html
│   ├── roles.html
│   └── usuarios.html
└── README.md
```

## Requisitos

- Visual Studio 2022
- .NET Framework 4.8
- SQL Server (local)

## Configuración en Visual Studio 2022

### 1. Abrir la solución
1. Abrir `PerfilUsuarioAPI.sln` con Visual Studio 2022
2. Si el proyecto aparece como "Descargado", clic derecho → Recargar proyecto

### 2. Restaurar paquetes NuGet
Clic derecho en la solución → **Restaurar paquetes NuGet**

Si no se restauran, en Package Manager Console (PM>):
```
PM> Install-Package Dapper
PM> Install-Package Microsoft.AspNet.WebApi.Cors
PM> Install-Package Swashbuckle
PM> Install-Package Newtonsoft.Json
```

### 3. Configurar conexión a BD
Verificar en `Web.config` que el `connectionString` apunte a tu SQL Server:
```xml
<connectionStrings>
  <add name="DefaultConnection"
       connectionString="Server=localhost;Database=PerfilUsuarioDB;Trusted_Connection=true;"
       providerName="System.Data.SqlClient" />
</connectionStrings>
```

### 4. Compilar y ejecutar
- Ctrl+Shift+B → Compilar
- F5 → Ejecutar con IIS Express
- Swagger disponible en: `https://localhost:{puerto}/swagger`

### 5. Configurar Frontend
1. Copiar el puerto de IIS Express (ej: `https://localhost:44367`)
2. Editar `Frontend/js/config.js`:
   ```js
   const API_BASE = 'https://localhost:44367/api';
   ```
3. Abrir `Frontend/login.html` en el navegador

## Base de Datos
Crear la base de datos `PerfilUsuarioDB` en SQL Server con las tablas.
**Insertar al menos un usuario manualmente** para poder hacer login:
```sql
INSERT INTO usuarios (username, password, suspendido) VALUES ('admin', 'admin123', 0);
```

Tablas:
- `usuarios` (id, username, password, suspendido)
- `roles` (id, strValor, strDescripcion)
- `UsuarioRoles` (id, idUsuario, idRol)
- `perfilUsuario` (id, nombre, apellidoPaterno, apellidoMaterno, fechaNacimiento, rfc, idUsuario)
- `Telefonos` (id, celular, casa, oficina, idPerfilUsuario)
- `direcciones` (id, calle, colonia, NumInterior, NumExterior, Municipio, idPerfilUsuario)

## Endpoints de la API

| Método | Ruta | Descripción |
|--------|------|-------------|
| POST | /api/auth/login | Iniciar sesión |
| GET | /api/perfilUsuario | Listar perfiles |
| GET | /api/perfilUsuario/{id} | Detalle de perfil |
| POST | /api/perfilUsuario | Crear perfil |
| PUT | /api/perfilUsuario/{id} | Actualizar perfil |
| DELETE | /api/perfilUsuario/{id} | Eliminar perfil |
| GET | /api/direcciones | Listar direcciones |
| GET | /api/direcciones/{id} | Detalle de dirección |
| POST | /api/direcciones | Crear dirección |
| PUT | /api/direcciones/{id} | Actualizar dirección |
| DELETE | /api/direcciones/{id} | Eliminar dirección |
| GET | /api/roles | Listar roles |
| GET | /api/roles?nombre=x | Buscar roles por nombre |
| GET | /api/roles/{id} | Detalle de rol |
| POST | /api/roles | Crear rol |
| PUT | /api/roles/{id} | Actualizar rol |
| DELETE | /api/roles/{id} | Eliminar rol |
| GET | /api/usuarios | Listar usuarios |
| GET | /api/usuarios/{id} | Detalle de usuario |
| POST | /api/usuarios | Crear usuario |
| PUT | /api/usuarios/{id} | Actualizar usuario |
| DELETE | /api/usuarios/{id} | Eliminar usuario |
