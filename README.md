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
│   ├── Global.asax.cs
│   └── Web.config              # Connection string aquí
├── Frontend/
│   ├── css/styles.css
│   ├── js/config.js            # URL de la API aquí
│   ├── login.html
│   ├── registro.html
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

### 1. Crear el proyecto
1. Abrir Visual Studio 2022
2. "Crear un proyecto" → **Aplicación web ASP.NET (.NET Framework)** (C#)
3. Nombre: `PerfilUsuarioAPI` | Framework: **.NET Framework 4.8**
4. Seleccionar plantilla **Web API** → Crear

### 2. Limpiar archivos del template
Eliminar del proyecto:
- `Controllers/HomeController.cs`
- `Controllers/ValuesController.cs`
- `Views/` (toda la carpeta)
- `Areas/` (si existe)

### 3. Copiar archivos del repositorio
Desde este repo, copiar y reemplazar:
- `PerfilUsuarioAPI/Controllers/` → los 5 archivos .cs
- `PerfilUsuarioAPI/Models/` → los 8 archivos .cs
- `PerfilUsuarioAPI/App_Start/WebApiConfig.cs` → reemplazar el existente
- `PerfilUsuarioAPI/Global.asax.cs` → reemplazar el existente
- `PerfilUsuarioAPI/Web.config` → agregar el `<connectionStrings>` (ver abajo)

### 4. Agregar archivos al proyecto
En VS: clic derecho en `Controllers/` → Agregar → Elemento existente → seleccionar los 5 .cs
Repetir con `Models/` → los 8 .cs

### 5. Instalar paquetes NuGet
En Package Manager Console (PM>):
```
PM> Install-Package Dapper
PM> Install-Package Microsoft.AspNet.WebApi.Cors
PM> Install-Package Swashbuckle
```

### 6. Configurar conexión a BD
En `Web.config`, agregar dentro de `<configuration>` (antes de `<appSettings>`):
```xml
<connectionStrings>
  <add name="DefaultConnection"
       connectionString="Server=localhost;Database=PerfilUsuarioDB;Trusted_Connection=true;"
       providerName="System.Data.SqlClient" />
</connectionStrings>
```

### 7. Compilar y ejecutar
- Ctrl+Shift+B → Compilar
- F5 → Ejecutar con IIS Express
- Swagger disponible en: `https://localhost:{puerto}/swagger`

### 8. Configurar Frontend
1. Copiar el puerto de IIS Express (ej: `https://localhost:44367`)
2. Editar `Frontend/js/config.js`:
   ```js
   const API_BASE = 'https://localhost:44367/api';
   ```
3. Abrir `Frontend/login.html` en el navegador

## Base de Datos
Crear la base de datos `PerfilUsuarioDB` en SQL Server con las tablas:
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
| POST | /api/usuarios/registro | Registrar usuario (público) |
| PUT | /api/usuarios/{id} | Actualizar usuario |
| DELETE | /api/usuarios/{id} | Eliminar usuario |
