# Roadmap

Este documento describe la hoja de ruta para el proyecto .NET Starter Kit, centrándose en la estrategia e implementación de pruebas.

## Estructura de Directorios de Pruebas

Se ha implementado una estructura de directorios para organizar los tests de manera clara y mantenible:

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

## Testing Strategy

### Unit Testing

- [x] Set up xUnit test projects
- [x] Implement test data builders
- [x] Create base test classes
- [x] Test domain entities and value objects
- [x] Add tests for domain services
- [x] Implement tests for application services
- [x] Add tests for API controllers
- [x] Implement TokenService tests with full coverage
- [x] Set up Moq for mocking dependencies
- [x] Add FluentAssertions for better test assertions
- [x] Implement central package management for all test projects
- [x] Standardize test project configurations
- [x] Update package references to use centralized versions

### Integration Testing

- [x] Set up test database infrastructure
- [x] Implement repository tests
- [x] Test database migrations
- [x] Test event handlers
- [x] Add tests for API endpoints
- [x] Test authentication and authorization
- [ ] Test background services

### End-to-End Testing

- [ ] Set up Playwright or Selenium
- [ ] Implement UI tests
- [ ] Test critical user journeys
- [ ] Add API contract tests

### Performance Testing

- [ ] Set up benchmarking
- [ ] Implement load tests
- [ ] Test database query performance
- [ ] Monitor memory usage

### Security Testing

- [ ] Implement security scanning
- [ ] Test for common vulnerabilities
- [ ] Add authentication and authorization tests
- [ ] Test input validation

## Current Focus

### Completed

- [x] Implement core domain model tests
- [x] Set up integration test infrastructure
- [x] Add repository tests
- [x] Implement event handler tests
- [x] Add API controller tests
- [x] Set up test coverage reporting
- [x] Implement TokenService unit tests
- [x] Organize test projects by feature and type

### Next Up

- [ ] Implement end-to-end tests
- [ ] Set up CI/CD pipeline for testing
- [ ] Add mutation testing
- [ ] Set up performance testing

## Future Enhancements

### Test Automation

- [ ] Implement test data management
- [ ] Add test parallelization
- [ ] Set up test reporting
- [ ] Add mutation testing

### Developer Experience

- [ ] Add test code snippets
- [ ] Implement test code generation
- [ ] Add documentation for testing patterns
- [ ] Create test templates

### Monitoring and Reporting

- [ ] Set up test result dashboards

- [ ] Implement test flakiness detection

- [ ] Add performance baselines

- [ ] Monitor test execution time

## Cómo Contribuir

1. Haz un fork del repositorio
2. Crea una rama de características (`git checkout -b feature/nueva-funcionalidad`)
3. Añade tus pruebas siguiendo los patrones establecidos
4. Asegúrate de que todas las pruebas pasan
5. Envía una pull request

### Directrices para Pruebas

- **Nombres de pruebas**: Usar el patrón `Método_Estado_ResultadoEsperado`
- **Estructura AAA**: Organizar las pruebas en secciones Arrange-Act-Assert
- **Fixtures**: Usar las clases base y fixtures proporcionadas
- **Cobertura**: Apuntar al menos al 80% de cobertura de código
- **Mocking**: Usar Moq para dependencias externas
- **Aserciones**: Usar FluentAssertions para aserciones legibles

## Resources

- [xUnit Documentation](https://xunit.net/)
- [FluentAssertions Documentation](https://fluentassertions.com/)
- [Moq Documentation](https://github.com/moq/moq4/wiki/Quickstart)
- [Bogus Documentation](https://github.com/bchavez/Bogus)
