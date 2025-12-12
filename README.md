# Sentara.Api

# Sentara – Sistema de Asistencia con Reconocimiento Facial  
Backend (.NET) + Frontend (HTML + face-api.js)

Sentara es un sistema híbrido para verificación inteligente de asistencia.  
El **frontend** realiza el reconocimiento facial en el navegador usando **face-api.js**, mientras que el **backend** almacena la asistencia, guarda capturas y envía correos al docente.


# Funcionalidades

### Frontend (Navegador)
- Usa `face-api.js` para:
  - Detección facial  
  - Puntos de referencia  
  - Obtención de descriptores  
  - Comparación vectorial entre foto oficial y captura en vivo  
- Valida coincidencia antes de permitir el envío  
- Solicita revisión manual después de 3 fallos  
- Envía capturas en Base64 al backend  
- Todo el reconocimiento ocurre **localmente**, sin cargar el servidor

### Backend (.NET Web API)
- Recibe asistencia y la guarda en SQL Server  
- Guarda capturas como archivos PNG  
- Envía correos con adjuntos al docente  
- Recibe solicitudes de revisión manual  
- Proporciona endpoint `/api/health` para verificar conexión  
- CORS habilitado para `http://127.0.0.1:5500` (Live Server)

#Requisitos

### Backend:
- **.NET 8 SDK**
- **SQL Server**
- Credenciales SMTP (Gmail recomendado con App Password)

### Frontend:
- Navegador moderno (Chrome)
- Live Server (VS Code) o servidor estático simple

---

# Cómo ejecutar el Backend

### Clonar el repositorio
```bash
git clone https://github.com/FanyNavas/Sentara.Api.git
cd Sentara.Api

2. Configurar appsettings.json
{
  "ConnectionStrings": {
    "SentaraDb": "Server=TU-SERVIDOR;Database=SentaraAttendance;Trusted_Connection=True;TrustServerCertificate=True;"
  },
  "EmailSettings": {
    "SmtpHost": "smtp.gmail.com",
    "SmtpPort": 587,
    "EnableSsl": true,
    "From": "tu@gmail.com",
    "User": "tu@gmail.com",
    "Password": "tu-app-password"
  }
}

3. Crear base de datos

Crear en SQL Server una base llamada SentaraAttendance.

4. Ejecutar
dotnet restore
dotnet build
dotnet run

---

El backend estará disponible en:

http://127.0.0.1:3001

**# Cómo ejecutar el Frontend**

Abrir el archivo HTML en VS Code

Click derecho → Open with Live Server

Se abrirá en:

http://127.0.0.1:5500


El backend ya permite este origen mediante CORS.

---

**###Función del Frontend**

Usa face-api.js para reconocimiento facial

Compara foto oficial vs cámara

Solo permite enviar la asistencia si hay coincidencia

Permite solicitar Revisión Manual después de 3 fallos

Envía capturas en Base64 al backend

El backend no realiza reconocimiento, solo guarda datos

.env.example

Opcional si usas variables de entorno:

CONNECTIONSTRINGS__SENTARADB=Server=TU-SERVER;Database=SentaraAttendance;Trusted_Connection=True;TrustServerCertificate=True;

EMAILSETTINGS__SMTPHOST=smtp.gmail.com
EMAILSETTINGS__SMTPPORT=587
EMAILSETTINGS__ENABLESSL=true
EMAILSETTINGS__FROM=tu@gmail.com
EMAILSETTINGS__USER=tu@gmail.com
EMAILSETTINGS__PASSWORD=tu-app-password

ASPNETCORE_ENVIRONMENT=Development
ASPNETCORE_URLS=http://127.0.0.1:3001

---

Historias de Usuario (HU)
ID	Como...	Quiero...	Para...
HU1	Docente	Registrar asistencia automáticamente	Ahorrar tiempo y evitar lista manual
HU2	Docente	Recibir correo con resumen y captura	Tener evidencia visual
HU3	Docente	Solicitar revisión manual tras fallos	Determinar si el alumno estuvo presente
HU4	Estudiante	Que mi asistencia se registre con mi rostro	Garantizar que mi presencia quede documentada
HU5	Administrador	Configurar DB/SMTP fácilmente	Implementaciones sin fricción
HU6	QA/Soporte	Verificar el backend con /api/health	Confirmar disponibilidad rápida
--
Arquitectura
Backend

Controllers: manejo de solicitudes HTTP

Domain: entidades AttendanceRecord, ManualReviewRequest

Infrastructure:

SentaraDbContext

IEmailSender + SmtpEmailSender

Carpeta Snapshots para almacenar imágenes

Frontend

Todo el reconocimiento facial ocurre en el navegador

Usa face-api.js

Controla estados, reintentos y verificación continua

Envía resultados al backend a través de JSON
---

**Resumen Final**

1. El frontend reconoce el rostro del estudiante usando face-api.js  
2. Solo si hay coincidencia se habilita **Submit**  
3. El frontend envía datos + captura al backend  
4. El backend guarda el registro y envía un correo al docente  
5. Si el reconocimiento falla repetidamente, se puede solicitar **Revisión Manual**  

Este archivo README proporciona toda la información necesaria para ejecutar, entender y evaluar el proyecto.
