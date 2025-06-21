# Estructura de Pruebas

Este directorio contiene todos los tests del proyecto, organizados en diferentes categorías según su propósito y alcance.

## Estructura de Directorios

```text
tests/
├── unit/                        # Pruebas unitarias
│   ├── Framework/               # Pruebas para el framework común
│   │   ├── Core.Tests/          # Pruebas para la lógica de negocio central
│   │   └── Infrastructure.Tests/ # Pruebas para la infraestructura compartida
│   └── Modules/                 # Pruebas por módulo
│       ├── Catalog/             
│       │   ├── Catalog.Application.Tests/  # Pruebas de servicios de aplicación
│       │   ├── Catalog.Domain.Tests/       # Pruebas de dominio
│       │   └── Catalog.Infrastructure.Tests/ # Pruebas de infraestructura
│       └── Todo.Tests/          # Pruebas para el módulo Todo
│
├── integration/               # Pruebas de integración
│   ├── API.Tests/              # Pruebas de integración de API
│   └── Modules.Tests/          # Pruebas de integración entre módulos
│
├── e2e/                       # Pruebas de extremo a extremo
│   └── UI.Tests/               # Pruebas de interfaz de usuario
│
└── shared/                    # Código compartido entre pruebas
    ├── TestCommon/             # Utilidades y ayudantes comunes
    └── TestFixtures/           # Fixtures y datos de prueba reutilizables
```

## Tipos de Pruebas

### Pruebas Unitarias (`unit/`)

#### Framework
- **Core.Tests**: Pruebas para la lógica de negocio central, incluyendo:
  - Entidades de dominio
  - Objetos de valor
  - Eventos de dominio
  - Excepciones personalizadas
  - Servicios de dominio

- **Infrastructure.Tests**: Pruebas para componentes de infraestructura compartidos:
  - Helpers y extensiones
  - Servicios de utilidad
  - Implementaciones de interfaces comunes
  - Configuraciones

#### Modules
Cada módulo sigue una estructura similar:

- **Catalog/**: Pruebas para el módulo de catálogo
  - `Catalog.Application.Tests`: 
    - Command/Query Handlers
    - Validadores
    - DTOs y mapeos
    - Servicios de aplicación
  - `Catalog.Domain.Tests`:
    - Entidades y agregados
    - Reglas de negocio
    - Especificaciones
    - Eventos de dominio
  - `Catalog.Infrastructure.Tests`:
    - Repositorios
    - Configuraciones de persistencia
    - Servicios de infraestructura
    - Integraciones externas

### Pruebas de Integración (`integration/`)

- **API.Tests**: 
  - Controladores
  - Middleware
  - Filtros
  - Autenticación y autorización
  - Rutas y enrutamiento

- **Modules.Tests**:
  - Integración entre servicios de diferentes módulos
  - Flujos de trabajo completos
  - Consistencia de datos entre contextos delimitados

### Pruebas de Extremo a Extremo (`e2e/`)

- **UI.Tests**:
  - Flujos de usuario críticos
  - Componentes de UI
  - Navegación
  - Formularios y validaciones
  - Pruebas de accesibilidad

### Código Compartido (`shared/`)

- **TestCommon**:
  - Clases base para pruebas
  - Helpers y extensiones
  - Fábricas de datos de prueba
  - Constantes y configuraciones

- **TestFixtures**:
  - Datos de prueba reutilizables
  - Contextos de prueba
  - Servicios mockeados
  - Configuraciones de prueba

## Convenciones de Nombrado

### Nombres de Clases de Prueba
- Para pruebas unitarias: `{ClaseBajoPrueba}Tests`
- Para pruebas de integración: `{Característica}IntegrationTests`
- Para pruebas E2E: `{Flujo}E2ETests`

### Nombres de Métodos de Prueba
Seguir el patrón: `Deberia_{ComportamientoEsperado}_Cuando_{Condicion}`

Ejemplo:
```csharp
[Fact]
public void Deberia_RetornarProducto_Cuando_ExisteEnElRepositorio()
{
    // Arrange
    // Act
    // Assert
}
```

## Mejores Prácticas

1. **Arrange-Act-Assert (AAA)**:
   - Separar claramente las secciones de configuración, ejecución y verificación.
   - Mantener las pruebas enfocadas en un solo comportamiento.

2. **Nombres Descriptivos**:
   - Usar nombres que describan el comportamiento probado.
   - Incluir el escenario y el resultado esperado.

3. **Pruebas Independientes**:
   - Cada prueba debe ser independiente de las demás.
   - No depender del estado de otras pruebas.

4. **Manejo de Datos de Prueba**:
   - Usar fábricas para crear datos de prueba.
   - Mantener los datos de prueba cercanos a las pruebas que los usan.

5. **Mocking**:
   - Mockear solo las dependencias externas necesarias.
   - Verificar las interacciones con los mocks.

## Ejecución de Pruebas

### Ejecutar Todas las Pruebas
```bash
dotnet test
```

### Ejecutar Pruebas Específicas
```bash
dotnet test --filter "FullyQualifiedName~Catalog.Application.Tests"
```

### Generar Informe de Cobertura
```bash
dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=cobertura
```

## Configuración Recomendada

### .editorconfig
Asegúrate de tener configurado el análisis de código en tu `.editorconfig`:

```ini
[*.cs]
dotnet_diagnostic.CA2007.severity = none
dotnet_style_qualification_for_field = false:warning
```

### Propiedades del Proyecto de Prueba
```xml
<PropertyGroup>
  <TargetFramework>net8.0</TargetFramework>
  <ImplicitUsings>enable</ImplicitUsings>
  <Nullable>enable</Nullable>
  <IsPackable>false</IsPackable>
  <IsTestProject>true</IsTestProject>
  <GenerateProgramFile>true</GenerateProgramFile>
</PropertyGroup>
```

## Recursos Adicionales

- [xUnit Documentation](https://xunit.net/)
- [FluentAssertions Documentation](https://fluentassertions.com/)
- [Moq Documentation](https://github.com/moq/moq4/wiki/Quickstart)
- [.NET Testing Guidelines](https://docs.microsoft.com/en-us/dotnet/core/testing/)
  - `Todo.Tests`: Pruebas para el módulo de tareas pendientes.

### Pruebas de Integración (`integration/`)
- `API.Tests`: Pruebas de integración para los endpoints de la API.
- `Modules.Tests`: Pruebas de integración entre diferentes módulos.

### Pruebas de Extremo a Extremo (`e2e/`)
- `UI.Tests`: Pruebas automatizadas de la interfaz de usuario.

### Código Compartido (`shared/`)
- `TestCommon`: Utilidades y ayudantes comunes para las pruebas.
- `TestFixtures`: Fixtures y datos de prueba reutilizables.

## Directrices para Escribir Pruebas

1. **Nombres de pruebas**: Usar el patrón `Método_Estado_ResultadoEsperado`.
2. **Estructura AAA**: Organizar las pruebas en secciones Arrange-Act-Assert.
3. **Fixtures**: Usar las clases base y fixtures proporcionadas.
4. **Cobertura**: Apuntar al menos al 80% de cobertura de código.
5. **Mocking**: Usar Moq para dependencias externas.
6. **Aserciones**: Usar FluentAssertions para aserciones legibles.

## Ejecutando las Pruebas

Para ejecutar todas las pruebas:

```bash
dotnet test
```

Para ejecutar pruebas específicas:

```bash
# Ejecutar pruebas unitarias
dotnet test tests/unit

# Ejecutar pruebas de integración
dotnet test tests/integration

# Ejecutar pruebas E2E
dotnet test tests/e2e
```

## Convenciones de Código

- Usar nombres descriptivos para las clases de prueba: `[ClaseBajoPrueba]Tests`
- Agrupar pruebas relacionadas en clases usando `[Trait("Category", "NombreCategoria")]`
- Usar `[Fact]` para pruebas sin parámetros y `[Theory]` para pruebas parametrizadas
- Documentar el propósito de cada prueba con comentarios claros

## Recursos

- [Documentación de xUnit](https://xunit.net/)
- [FluentAssertions](https://fluentassertions.com/)
- [Moq](https://github.com/moq/moq4/wiki/Quickstart)
