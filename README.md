## ⚙️ Instalación y Despliegue Local

Sigue estos pasos para clonar y ejecutar el proyecto Gymphony en tu entorno local:

### 1. Requisitos Previos
* Visual Studio (2022 o superior) con la carga de trabajo de desarrollo web y ASP.NET.
* SQL Server y SQL Server Management Studio (SSMS).
* .NET SDK instalado.

### 2. Clonar el Repositorio
Abre Visual Studio, selecciona **"Clonar un repositorio"** y pega la URL de este proyecto en GitHub.

### 3. Configuración de la Base de Datos
1. Ve a la carpeta `scripts` dentro del repositorio clonado y abre el archivo `script.sql`.
2. Abre **SQL Server Management Studio (SSMS)** y conéctate a tu servidor local (`localhost`).
3. Crea una base de datos vacía llamada `Gymphony`.
4. Abre una nueva consulta (*New Query*) apuntando a la base de datos `Gymphony`, pega el contenido de `script.sql` y ejecútalo. Esto creará todas las tablas, vistas, relaciones y datos de prueba (inserts).

### 4. Configurar la Cadena de Conexión
Para que la aplicación se conecte a tu base de datos recién creada, abre el archivo `appsettings.json` en Visual Studio y actualiza la sección `"SqlGymphony"` con tu cadena de conexión:

```json
"ConnectionStrings": {
  "SqlGymphony": "Data Source=TU_SERVIDOR;Initial Catalog=Gymphony;Persist Security Info=True;User ID=tu_usuario;Encrypt=True;Trust Server Certificate=True"
}
```

> **💡 Truco rápido para obtener tu cadena de conexión:** > Si no te sabes tu cadena de conexión de memoria, en Visual Studio ve a **Ver > Explorador de servidores** (*Server Explorer*). Haz clic en el icono de *Conectar a la base de datos*, introduce los datos de tu servidor local (Autenticación de SQL Server, usuario y contraseña) y selecciona la base de datos `Gymphony`.
> Una vez conectada, haz clic derecho sobre ella, ve a **Propiedades**, copia el valor exacto de `Connection String` y pégalo en tu `appsettings.json`.

### 5. Ejecutar el Proyecto
Con la base de datos configurada, selecciona el perfil de ejecución en Visual Studio y presiona **F5** (o haz clic en el botón de "Iniciar"). La aplicación se compilará y se abrirá en tu navegador predeterminado.

---

## 🔑 Usuarios de Prueba

El script de la base de datos incluye registros de prueba para que puedas explorar las distintas funcionalidades de la aplicación según el nivel de acceso. Utiliza las siguientes credenciales para iniciar sesión:

| Rol | Email | Contraseña |
| :--- | :--- | :--- |
| 👑 **Administrador** | `admin@gmail.com` | `12345` |
| 🏋️‍♂️ **Entrenador** | `marcosTrainer@gmail.com` | `12345` |
| 🏃 **Socio** | `alonso@gmail.com` | `12345` |

*(Nota: Tienes más usuarios disponibles en la tabla `Usuarios` de la base de datos, todos con la misma contraseña de pruebas).*
