# Sistema de Perfiles de Usuario

REST API con .NET 8 Web API + Dapper y Frontend en HTML/CSS/JS vanilla.

## Estructura del Proyecto

```
perfilUsuario/
├── PerfilUsuarioAPI/          ← Backend (.NET Web API)
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
│   ├── appsettings.json
│   ├── Program.cs
│   └── PerfilUsuarioAPI.csproj
├── Frontend/                  ← Frontend (HTML/CSS/JS)
│   ├── css/styles.css
│   ├── js/config.js
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

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- SQL Server con la base de datos ya creada

## Configuración

1. Edita la cadena de conexión en `PerfilUsuarioAPI/appsettings.json`:

```json
"ConnectionStrings": {
    "DefaultConnection": "Server=TU_SERVIDOR;Database=TU_BD;Trusted_Connection=true;TrustServerCertificate=true;"
}
```

2. Si la API no corre en `http://localhost:5000`, actualiza `Frontend/js/config.js`:

```js
const API_BASE = 'http://localhost:5000/api';
```

## Ejecución

### Backend

```bash
cd PerfilUsuarioAPI
dotnet restore
dotnet run
```

La API estará disponible en `http://localhost:5000` (o el puerto configurado).

### Frontend

Abre `Frontend/login.html` directamente en el navegador o usa un servidor local:

```bash
cd Frontend
# Opción 1: Python
python3 -m http.server 8080

# Opción 2: Node.js (npx)
npx serve .
```

Luego navega a `http://localhost:8080/login.html`.

## Endpoints de la API

| Método | Ruta | Descripción |
|--------|------|-------------|
| POST | /api/auth/login | Login de usuario |
| GET | /api/perfilUsuario | Todos los perfiles (con dirección y teléfono) |
| GET | /api/perfilUsuario/{id} | Perfil por ID |
| POST | /api/perfilUsuario | Crear perfil + dirección + teléfono |
| PUT | /api/perfilUsuario/{id} | Actualizar perfil |
| DELETE | /api/perfilUsuario/{id} | Eliminar perfil en cascada |
| GET | /api/direcciones | Todas las direcciones |
| GET | /api/direcciones/{id} | Dirección por ID |
| POST | /api/direcciones | Crear dirección |
| PUT | /api/direcciones/{id} | Actualizar dirección |
| DELETE | /api/direcciones/{id} | Eliminar dirección |
| GET | /api/roles | Todos los roles (o filtrar con ?nombre=) |
| POST | /api/roles | Crear rol |
| PUT | /api/roles/{id} | Actualizar rol |
| DELETE | /api/roles/{id} | Eliminar rol |
| GET | /api/usuarios | Todos los usuarios con roles |
| GET | /api/usuarios/{id} | Usuario por ID con roles |
| POST | /api/usuarios | Crear usuario con roles |
| PUT | /api/usuarios/{id} | Actualizar usuario y roles |
| DELETE | /api/usuarios/{id} | Eliminar usuario |
