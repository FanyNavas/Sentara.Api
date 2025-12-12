# Sentara.Api

# Sentara – Sistema de Asistencia con Reconocimiento Facial  
Backend (.NET) + Frontend (HTML + face-api.js)

Sentara es un sistema híbrido para verificación inteligente de asistencia.  
El **frontend** realiza el reconocimiento facial en el navegador usando **face-api.js**, mientras que el **backend** almacena la asistencia, guarda capturas y envía correos al docente.

---

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

---

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
git clone
cd Sentara.Api
