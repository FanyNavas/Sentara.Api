# SOLID en Sentara.Api

Este documento resume cómo se aplican los principios SOLID en el backend de **Sentara** y apunta a las clases donde se ve cada uno.

---

## S — Single Responsibility Principle (SRP)

> Una clase debe tener una sola razón para cambiar.

### Evidencias

- **`Sentara.Api.Infrastructure.SmtpEmailSender`** (`Infrastructure/SmtpEmailSender.cs`)  
  - Responsabilidad única: enviar correos electrónicos usando SMTP.
  - No sabe nada de HTTP, controladores, ni base de datos.

- **`Sentara.Api.Infrastructure.SentaraDbContext`** (`Infrastructure/SentaraDbContext.cs`)  
  - Responsabilidad: exponer el modelo de datos (`AttendanceRecord`, `ManualReviewRequest`) para EF Core.

- **Entidades de dominio**
  - `Sentara.Api.Domain.AttendanceRecord` (`Domain/AttendanceRecord.cs`)  
  - `Sentara.Api.Domain.ManualReviewRequest` (`Domain/ManualReviewRequest.cs`)  
  - Cada clase solo representa un tipo de registro de negocio (asistencia, revisión manual).

- **DTOs**
  - `SubmitAttendanceRequest` (`DTOs/SubmitAttendanceRequest.cs`)  
  - `ManualReviewRequestDto` (`DTOs/ManualReviewRequestDto.cs`)  
  - `ApiResponse` (`DTOs/ApiResponse.cs`)  
  - Responsabilidad: transportar datos de/para la API, sin lógica de negocio.

- **Configuraciones / Infraestructura simple**
  - `EmailSettings` (`Infrastructure/EmailSettings.cs`)  
  - `EmailAttachment` (`Infrastructure/EmailAttachment.cs`)  
  - Cada clase encapsula un solo concepto de configuración/datos auxiliares.

### Áreas a mejorar

- **`AttendanceController`** (`Controllers/AttendanceController.cs`)
  - Actualmente mezcla:
    - Validación de entrada.
    - Lógica de negocio (crear entidades).
    - Acceso a datos (`SentaraDbContext`).
    - Orquestación de envío de correos.
  - Idealmente, parte de esta lógica debería moverse a un **servicio de aplicación** (por ejemplo, `IAttendanceService`) para que el controlador solo coordine la petición y la respuesta.

---

## O — Open/Closed Principle (OCP)

> Las entidades de software deben estar abiertas para extensión, pero cerradas para modificación.

### Evidencias

- **`IEmailSender` + `SmtpEmailSender`**
  - El código del controlador depende de `IEmailSender`, no de `SmtpEmailSender`.
  - Para cambiar el proveedor de correo (por ejemplo, un servicio HTTP externo), se puede agregar otra implementación (`SendGridEmailSender`, `FakeEmailSender`) **sin modificar** el controlador.

### Áreas a mejorar

- No existen aún **jerarquías de tipos** o puntos de extensión claros para la lógica de asistencia:
  - No hay servicios de dominio o de aplicación donde se pueda aplicar OCP de manera más rica (por ejemplo, distintas estrategias de validación o notificación).
- Una mejora futura:
  - Crear un servicio `IAttendanceService` con implementación `AttendanceService`.
  - Permitir diferentes estrategias de notificación (correo, notificación interna, etc.) sin tocar el controlador.

---

## L — Liskov Substitution Principle (LSP)

> Los objetos de una clase derivada deben poder sustituir a los de su clase base sin romper la lógica.

### Evidencias

- Por ahora, el código no usa **herencia de clases** en el dominio o servicios (todas las clases relevantes son concretas).
- LSP se cumple de forma trivial al no tener jerarquías de herencia.

### Áreas a mejorar

- Aún no hay ejemplos fuertes de LSP (por ejemplo, `BaseEmailSender` con varias subclases).
- Futuro posible:
  - Si se crea una jerarquía de servicios de correo o validadores, asegurar que cualquier clase derivada respete el contrato de la base.

---

## I — Interface Segregation Principle (ISP)

> Los clientes no deberían verse forzados a depender de interfaces que no usan.

### Evidencias

- **`IEmailSender`** (`Infrastructure/IEmailSender.cs`)
  - Es una interfaz pequeña y específica, con un solo método `SendAsync(...)`.
  - Los consumidores (controladores o servicios) solo dependen de la funcionalidad que realmente necesitan: enviar un correo (con attachments opcionales).

### Áreas a mejorar

- Aunque `IEmailSender` cumple ISP, no existen más interfaces especializadas:
  - No hay interfaces separadas para **persistencia**, **lógica de negocio de asistencia**, etc.
- Mejoras posibles:
  - Definir interfaces como `IAttendanceRepository`, `IManualReviewRepository`, `IAttendanceService`, etc., para dividir responsabilidades y evitar interfaces “gordas”.

---

## D — Dependency Inversion Principle (DIP)

> Los módulos de alto nivel no deben depender de módulos de bajo nivel; ambos deben depender de abstracciones.

### Evidencias

- **Uso de DI en `Program.cs`**
  - `builder.Services.AddDbContext<SentaraDbContext>(...)`
  - `builder.Services.Configure<EmailSettings>(...)`
  - `builder.Services.AddScoped<IEmailSender, SmtpEmailSender>();`
  - El contenedor de DI se encarga de crear e inyectar dependencias.

- **`AttendanceController`** (`Controllers/AttendanceController.cs`)
  - Recibe en el constructor:
    - `SentaraDbContext` (infraestructura / datos).
    - `IEmailSender` (abstracción de envío de correo).
    - `IWebHostEnvironment`, `ILogger<AttendanceController>`.
  - El controlador depende de la abstracción `IEmailSender`, no de `SmtpEmailSender` directamente.

### Áreas a mejorar

- Aunque el controlador depende de abstracciones para el correo, aún depende directamente de `SentaraDbContext`:
  - Idealmente, debería depender de un **servicio** o **repositorio** (por ejemplo, `IAttendanceRepository`, `IAttendanceService`) para no acoplarse a EF Core.
- Futuro:
  - Introducir capa de aplicación/servicios, para que los controladores solo dependan de interfaces de dominio/aplicación.

---

## Resumen

- **SRP:** Parcialmente aplicado (clases pequeñas y enfocadas, pero el controlador hace demasiado).
- **OCP:** Se ve un caso claro en `IEmailSender` / `SmtpEmailSender`; se puede mejorar para la lógica de negocio.
- **LSP:** Trivialmente cumplido (no hay herencia significativa aún).
- **ISP:** Aplicado en `IEmailSender`; faltan más interfaces específicas en la capa de dominio/aplicación.
- **DIP:** Aplicado a través de DI (`IEmailSender`), pero el controlador sigue dependiendo directamente de `SentaraDbContext`.

Este documento puede actualizarse a medida que se agreguen:
- Servicios de aplicación (por ejemplo, `AttendanceService`).
- Repositorios (`IAttendanceRepository`, `IManualReviewRepository`).
- Más interfaces y patrones que refuercen SOLID en el proyecto.


