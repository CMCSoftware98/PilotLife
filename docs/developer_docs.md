# PilotLife Developer Documentation

## Table of Contents

1. [Development Philosophy](#development-philosophy)
2. [Architecture Principles](#architecture-principles)
3. [.NET Development Standards](#net-development-standards)
4. [TypeScript & Vue Standards](#typescript--vue-standards)
5. [Testing Standards](#testing-standards)
6. [Database & Query Efficiency](#database--query-efficiency)
7. [API Design Standards](#api-design-standards)
8. [Error Handling & Logging](#error-handling--logging)
9. [Security Standards](#security-standards)
10. [Code Review & Git Workflow](#code-review--git-workflow)

---

## Development Philosophy

### Test-Driven Development (TDD)

**All code MUST be developed using Test-Driven Development.** This is non-negotiable.

```
TDD Cycle:
1. RED    → Write a failing test that defines desired behavior
2. GREEN  → Write the minimum code to make the test pass
3. REFACTOR → Improve the code while keeping tests green
```

**Benefits enforced by TDD:**
- Forces clear understanding of requirements before coding
- Produces testable, loosely-coupled code by design
- Creates living documentation through tests
- Catches regressions immediately
- Enables confident refactoring

**TDD Rules:**
1. Never write production code without a failing test
2. Write only enough test code to fail (compilation failures count)
3. Write only enough production code to pass the test
4. Refactor only when tests are green

### SOLID Principles

All code must adhere to SOLID principles:

| Principle | Description | Enforcement |
|-----------|-------------|-------------|
| **S**ingle Responsibility | A class should have only one reason to change | One service = one business capability |
| **O**pen/Closed | Open for extension, closed for modification | Use interfaces and inheritance appropriately |
| **L**iskov Substitution | Subtypes must be substitutable for base types | Ensure derived classes honor contracts |
| **I**nterface Segregation | Many specific interfaces > one general interface | Small, focused interfaces |
| **D**ependency Inversion | Depend on abstractions, not concretions | All dependencies injected via interfaces |

### Loose Coupling & High Cohesion

**Loose Coupling:**
- Components should have minimal knowledge of other components
- Communication through well-defined interfaces only
- No direct instantiation of dependencies (use DI)
- Event-driven communication where appropriate

**High Cohesion:**
- Related functionality grouped together
- Each module/class has a clear, focused purpose
- Avoid "god classes" that do everything

---

## Architecture Principles

### Clean Architecture

```
┌─────────────────────────────────────────────────────────────┐
│                      PRESENTATION                            │
│              (Controllers, ViewModels, UI)                   │
├─────────────────────────────────────────────────────────────┤
│                      APPLICATION                             │
│         (Services, DTOs, Interfaces, Use Cases)              │
├─────────────────────────────────────────────────────────────┤
│                        DOMAIN                                │
│            (Entities, Value Objects, Domain Events)          │
├─────────────────────────────────────────────────────────────┤
│                     INFRASTRUCTURE                           │
│      (Database, External Services, File System, APIs)        │
└─────────────────────────────────────────────────────────────┘

Dependencies flow INWARD only. Inner layers never reference outer layers.
```

### Project Structure

```
PilotLife/
├── src/
│   ├── PilotLife.API/              # Presentation layer
│   │   ├── Controllers/
│   │   ├── Middleware/
│   │   ├── Filters/
│   │   └── Program.cs
│   │
│   ├── PilotLife.Application/      # Application layer
│   │   ├── Services/
│   │   ├── Interfaces/
│   │   ├── DTOs/
│   │   ├── Validators/
│   │   └── Mappings/
│   │
│   ├── PilotLife.Domain/           # Domain layer
│   │   ├── Entities/
│   │   ├── ValueObjects/
│   │   ├── Enums/
│   │   ├── Events/
│   │   └── Exceptions/
│   │
│   └── PilotLife.Infrastructure/   # Infrastructure layer
│       ├── Data/
│       │   ├── Configurations/
│       │   ├── Repositories/
│       │   └── DbContext.cs
│       ├── Services/
│       └── Extensions/
│
├── tests/
│   ├── PilotLife.UnitTests/
│   ├── PilotLife.IntegrationTests/
│   └── PilotLife.E2ETests/
│
└── docs/
```

### Dependency Injection

**Every dependency MUST be injected through interfaces.**

```csharp
// BAD - Direct instantiation
public class JobService
{
    private readonly JobRepository _repository = new JobRepository();
}

// GOOD - Interface injection
public class JobService : IJobService
{
    private readonly IJobRepository _repository;
    private readonly IAuditService _auditService;
    private readonly ILogger<JobService> _logger;

    public JobService(
        IJobRepository repository,
        IAuditService auditService,
        ILogger<JobService> logger)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _auditService = auditService ?? throw new ArgumentNullException(nameof(auditService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }
}
```

**Service Registration Pattern:**
```csharp
// In ServiceCollectionExtensions.cs
public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddScoped<IJobService, JobService>();
        services.AddScoped<IAircraftService, AircraftService>();
        services.AddScoped<IExperienceService, ExperienceService>();
        // ... etc

        return services;
    }

    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services)
    {
        services.AddScoped<IJobRepository, JobRepository>();
        services.AddScoped<IAircraftRepository, AircraftRepository>();
        // ... etc

        return services;
    }
}
```

---

## .NET Development Standards

### Naming Conventions (Microsoft Standards)

| Element | Convention | Example |
|---------|------------|---------|
| Namespace | PascalCase | `PilotLife.Application.Services` |
| Class | PascalCase | `JobCompletionService` |
| Interface | IPascalCase | `IJobCompletionService` |
| Method | PascalCase | `CompleteJobAsync` |
| Property | PascalCase | `JobPayout` |
| Private field | _camelCase | `_jobRepository` |
| Parameter | camelCase | `jobId` |
| Local variable | camelCase | `completedJob` |
| Constant | PascalCase | `MaxRetryAttempts` |
| Enum | PascalCase | `JobStatus.Completed` |
| Async methods | Suffix with Async | `GetJobByIdAsync` |

### Interface Design

**Every service, repository, and external dependency MUST have an interface.**

```csharp
// Interface in Application layer
public interface IJobService
{
    Task<JobDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<PagedResult<JobDto>> GetAvailableJobsAsync(JobQueryParameters parameters, CancellationToken cancellationToken = default);
    Task<JobDto> AcceptJobAsync(Guid jobId, Guid playerId, CancellationToken cancellationToken = default);
    Task<JobCompletionResult> CompleteJobAsync(Guid jobId, FlightCompletionData data, CancellationToken cancellationToken = default);
}

// Implementation in Application layer (or Infrastructure for data access)
public class JobService : IJobService
{
    // Implementation
}
```

### Repository Pattern

**All data access MUST go through repositories.**

```csharp
// Generic repository interface
public interface IRepository<T> where T : class, IEntity
{
    Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<T>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<T> AddAsync(T entity, CancellationToken cancellationToken = default);
    Task UpdateAsync(T entity, CancellationToken cancellationToken = default);
    Task DeleteAsync(T entity, CancellationToken cancellationToken = default);
}

// Specific repository with domain-specific queries
public interface IJobRepository : IRepository<Job>
{
    Task<PagedResult<Job>> GetAvailableJobsAsync(
        Guid worldId,
        string? departureIcao,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Job>> GetPlayerActiveJobsAsync(
        Guid playerWorldId,
        CancellationToken cancellationToken = default);
}
```

### DTOs and Mapping

**Never expose entities directly to API consumers. Always use DTOs.**

```csharp
// Entity (Domain layer)
public class Job : IEntity
{
    public Guid Id { get; set; }
    public Guid WorldId { get; set; }
    public string DepartureIcao { get; set; }
    public string ArrivalIcao { get; set; }
    public decimal Payout { get; set; }
    public JobStatus Status { get; set; }
    // ... navigation properties, internal fields
}

// DTO (Application layer)
public record JobDto
{
    public Guid Id { get; init; }
    public string DepartureIcao { get; init; }
    public string ArrivalIcao { get; init; }
    public decimal Payout { get; init; }
    public string Status { get; init; }
    public double DistanceNm { get; init; }
}

// Request DTO
public record AcceptJobRequest
{
    public Guid JobId { get; init; }
    public Guid? RentalAircraftId { get; init; }
}

// Response DTO
public record AcceptJobResponse
{
    public JobDto Job { get; init; }
    public DateTime AcceptedAt { get; init; }
    public DateTime ExpiresAt { get; init; }
}
```

**Use AutoMapper or Mapster for object mapping:**
```csharp
public class JobMappingProfile : Profile
{
    public JobMappingProfile()
    {
        CreateMap<Job, JobDto>()
            .ForMember(d => d.Status, opt => opt.MapFrom(s => s.Status.ToString()));

        CreateMap<CreateJobRequest, Job>()
            .ForMember(d => d.Id, opt => opt.Ignore())
            .ForMember(d => d.Status, opt => opt.MapFrom(_ => JobStatus.Available));
    }
}
```

### Async/Await Standards

**All I/O operations MUST be async.**

```csharp
// GOOD - Async all the way
public async Task<JobDto> GetJobByIdAsync(Guid id, CancellationToken cancellationToken)
{
    var job = await _repository.GetByIdAsync(id, cancellationToken);
    return _mapper.Map<JobDto>(job);
}

// BAD - Blocking on async
public JobDto GetJobById(Guid id)
{
    var job = _repository.GetByIdAsync(id).Result; // NEVER DO THIS
    return _mapper.Map<JobDto>(job);
}

// GOOD - CancellationToken propagation
public async Task<PagedResult<JobDto>> GetJobsAsync(
    JobQueryParameters parameters,
    CancellationToken cancellationToken = default)
{
    var jobs = await _repository.GetAvailableJobsAsync(
        parameters.WorldId,
        parameters.DepartureIcao,
        parameters.Page,
        parameters.PageSize,
        cancellationToken); // Always propagate

    return new PagedResult<JobDto>
    {
        Items = _mapper.Map<List<JobDto>>(jobs.Items),
        TotalCount = jobs.TotalCount,
        Page = jobs.Page,
        PageSize = jobs.PageSize
    };
}
```

### Date & Time Handling

**ALWAYS use `DateTimeOffset` instead of `DateTime` for all timestamps.**

`DateTimeOffset` preserves timezone information and ensures consistent behavior across different server locations and client timezones.

```csharp
// BAD - DateTime loses timezone context
public class Job
{
    public DateTime CreatedAt { get; set; }
    public DateTime ExpiresAt { get; set; }
}

// GOOD - DateTimeOffset preserves timezone information
public class Job
{
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset ExpiresAt { get; set; }
}
```

**When to use each:**

| Type | Use Case |
|------|----------|
| `DateTimeOffset` | All timestamps (created, modified, expires, scheduled) |
| `DateTimeOffset` | API request/response timestamps |
| `DateTimeOffset` | Database columns for temporal data |
| `DateOnly` | Dates without time (birth date, expiry date) |
| `TimeOnly` | Times without date (daily schedule, opening hours) |
| `TimeSpan` | Durations (flight time, cooldown period) |

**Standards:**

```csharp
// Always use UTC for storage and calculations
public class Entity
{
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? ModifiedAt { get; set; }
}

// Service layer - always work in UTC
public async Task<Job> CreateJobAsync(CreateJobRequest request)
{
    var job = new Job
    {
        CreatedAt = DateTimeOffset.UtcNow,
        ExpiresAt = DateTimeOffset.UtcNow.AddHours(request.ExpiryHours)
    };

    return await _repository.AddAsync(job);
}

// API responses - return UTC, let client convert to local
public record JobDto
{
    public Guid Id { get; init; }
    public DateTimeOffset CreatedAt { get; init; }  // ISO 8601 with offset
    public DateTimeOffset ExpiresAt { get; init; }
}

// Comparing timestamps
public bool IsExpired(Job job)
{
    return job.ExpiresAt < DateTimeOffset.UtcNow;
}
```

**Database Configuration (EF Core):**

```csharp
public class JobConfiguration : IEntityTypeConfiguration<Job>
{
    public void Configure(EntityTypeBuilder<Job> builder)
    {
        // SQL Server stores as datetimeoffset(7)
        builder.Property(j => j.CreatedAt)
            .HasColumnType("datetimeoffset");

        builder.Property(j => j.ExpiresAt)
            .HasColumnType("datetimeoffset");
    }
}
```

**TypeScript/Frontend:**

```typescript
// Always parse as Date objects (handles ISO 8601 automatically)
interface Job {
  id: string;
  createdAt: string;  // ISO 8601 string from API
  expiresAt: string;
}

// Display in user's local timezone
const formatDateTime = (isoString: string): string => {
  return new Intl.DateTimeFormat(navigator.language, {
    dateStyle: 'medium',
    timeStyle: 'short',
    timeZone: Intl.DateTimeFormat().resolvedOptions().timeZone
  }).format(new Date(isoString));
};

// For calculations, work with Date objects
const isExpired = (job: Job): boolean => {
  return new Date(job.expiresAt) < new Date();
};
```

**JSON Serialization:**

```csharp
// System.Text.Json configuration
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        // Ensures ISO 8601 format with timezone offset
        options.JsonSerializerOptions.Converters.Add(
            new JsonStringEnumConverter());
        // DateTimeOffset serializes correctly by default
    });
```

**Example API Response:**
```json
{
  "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "createdAt": "2024-01-15T14:30:00+00:00",
  "expiresAt": "2024-01-16T14:30:00+00:00"
}
```

### Null Handling

**Use nullable reference types (NRT) and handle nulls explicitly.**

```csharp
// Enable in .csproj
<Nullable>enable</Nullable>

// Explicit nullability
public async Task<JobDto?> GetByIdAsync(Guid id)
{
    var job = await _repository.GetByIdAsync(id);

    if (job is null)
        return null;

    return _mapper.Map<JobDto>(job);
}

// Guard clauses
public async Task CompleteJobAsync(Guid jobId, FlightData data)
{
    ArgumentNullException.ThrowIfNull(data);

    var job = await _repository.GetByIdAsync(jobId)
        ?? throw new NotFoundException($"Job {jobId} not found");

    // Continue processing
}
```

---

## TypeScript & Vue Standards

### TypeScript Configuration

```json
// tsconfig.json - Strict mode REQUIRED
{
  "compilerOptions": {
    "strict": true,
    "noImplicitAny": true,
    "strictNullChecks": true,
    "strictFunctionTypes": true,
    "strictBindCallApply": true,
    "strictPropertyInitialization": true,
    "noImplicitThis": true,
    "noImplicitReturns": true,
    "noFallthroughCasesInSwitch": true,
    "noUncheckedIndexedAccess": true,
    "exactOptionalPropertyTypes": true
  }
}
```

### Naming Conventions

| Element | Convention | Example |
|---------|------------|---------|
| Component | PascalCase | `JobListView.vue` |
| Composable | camelCase with use prefix | `useJobService.ts` |
| Service | camelCase | `jobService.ts` |
| Interface | PascalCase with I prefix | `IJob`, `IJobService` |
| Type | PascalCase | `JobStatus`, `ApiResponse` |
| Constant | SCREAMING_SNAKE_CASE | `MAX_RETRY_ATTEMPTS` |
| Function | camelCase | `fetchJobs`, `calculatePayout` |
| Variable | camelCase | `jobList`, `isLoading` |

### Type Safety

**Never use `any`. Define proper types for everything.**

```typescript
// BAD
const fetchJobs = async (): Promise<any> => {
  const response = await api.get('/jobs');
  return response.data;
};

// GOOD
interface Job {
  id: string;
  departureIcao: string;
  arrivalIcao: string;
  payout: number;
  status: JobStatus;
}

interface PagedResponse<T> {
  items: T[];
  totalCount: number;
  page: number;
  pageSize: number;
}

const fetchJobs = async (params: JobQueryParams): Promise<PagedResponse<Job>> => {
  const response = await api.get<PagedResponse<Job>>('/jobs', { params });
  return response.data;
};
```

### Vue Component Standards

```vue
<!-- JobCard.vue -->
<script setup lang="ts">
import { computed } from 'vue';
import type { Job } from '@/types/job';

// Props with TypeScript
interface Props {
  job: Job;
  isSelected?: boolean;
}

const props = withDefaults(defineProps<Props>(), {
  isSelected: false,
});

// Emits with TypeScript
interface Emits {
  (e: 'select', jobId: string): void;
  (e: 'accept', jobId: string): void;
}

const emit = defineEmits<Emits>();

// Computed properties
const formattedPayout = computed(() =>
  new Intl.NumberFormat('en-US', {
    style: 'currency',
    currency: 'USD'
  }).format(props.job.payout)
);

// Methods
const handleAccept = (): void => {
  emit('accept', props.job.id);
};
</script>

<template>
  <div
    :class="['job-card', { 'job-card--selected': isSelected }]"
    @click="emit('select', job.id)"
  >
    <h3>{{ job.departureIcao }} → {{ job.arrivalIcao }}</h3>
    <p class="payout">{{ formattedPayout }}</p>
    <button @click.stop="handleAccept">Accept Job</button>
  </div>
</template>
```

### API Service Pattern

```typescript
// services/api.ts - Base API configuration
import axios, { AxiosInstance, AxiosRequestConfig } from 'axios';

class ApiService {
  private readonly client: AxiosInstance;

  constructor() {
    this.client = axios.create({
      baseURL: import.meta.env.VITE_API_URL,
      timeout: 30000,
      headers: {
        'Content-Type': 'application/json',
      },
    });

    this.setupInterceptors();
  }

  private setupInterceptors(): void {
    this.client.interceptors.request.use((config) => {
      const token = localStorage.getItem('auth_token');
      if (token) {
        config.headers.Authorization = `Bearer ${token}`;
      }
      return config;
    });

    this.client.interceptors.response.use(
      (response) => response,
      (error) => {
        // Handle errors globally
        if (error.response?.status === 401) {
          // Handle unauthorized
        }
        return Promise.reject(error);
      }
    );
  }

  async get<T>(url: string, config?: AxiosRequestConfig): Promise<T> {
    const response = await this.client.get<T>(url, config);
    return response.data;
  }

  async post<T, D = unknown>(url: string, data?: D, config?: AxiosRequestConfig): Promise<T> {
    const response = await this.client.post<T>(url, data, config);
    return response.data;
  }

  // ... put, patch, delete methods
}

export const api = new ApiService();
```

```typescript
// services/jobService.ts - Domain-specific service
import { api } from './api';
import type { Job, JobQueryParams, PagedResponse, AcceptJobRequest, AcceptJobResponse } from '@/types';

export interface IJobService {
  getJobs(params: JobQueryParams): Promise<PagedResponse<Job>>;
  getJobById(id: string): Promise<Job>;
  acceptJob(request: AcceptJobRequest): Promise<AcceptJobResponse>;
}

class JobService implements IJobService {
  async getJobs(params: JobQueryParams): Promise<PagedResponse<Job>> {
    return api.get<PagedResponse<Job>>(`/worlds/${params.worldId}/jobs`, {
      params: {
        page: params.page,
        pageSize: params.pageSize,
        departureIcao: params.departureIcao,
      },
    });
  }

  async getJobById(id: string): Promise<Job> {
    return api.get<Job>(`/jobs/${id}`);
  }

  async acceptJob(request: AcceptJobRequest): Promise<AcceptJobResponse> {
    return api.post<AcceptJobResponse>(`/jobs/${request.jobId}/accept`, request);
  }
}

export const jobService: IJobService = new JobService();
```

### Composables Pattern

```typescript
// composables/useJobs.ts
import { ref, computed, type Ref } from 'vue';
import { jobService } from '@/services/jobService';
import type { Job, JobQueryParams, PagedResponse } from '@/types';

interface UseJobsReturn {
  jobs: Ref<Job[]>;
  totalCount: Ref<number>;
  isLoading: Ref<boolean>;
  error: Ref<string | null>;
  hasMore: Computed<boolean>;
  fetchJobs: (params: JobQueryParams) => Promise<void>;
  fetchMore: () => Promise<void>;
  acceptJob: (jobId: string) => Promise<void>;
}

export function useJobs(worldId: string): UseJobsReturn {
  const jobs = ref<Job[]>([]);
  const totalCount = ref(0);
  const currentPage = ref(1);
  const pageSize = ref(20);
  const isLoading = ref(false);
  const error = ref<string | null>(null);

  const hasMore = computed(() => jobs.value.length < totalCount.value);

  const fetchJobs = async (params: JobQueryParams): Promise<void> => {
    isLoading.value = true;
    error.value = null;

    try {
      const response = await jobService.getJobs({
        ...params,
        worldId,
        page: 1,
        pageSize: pageSize.value,
      });

      jobs.value = response.items;
      totalCount.value = response.totalCount;
      currentPage.value = 1;
    } catch (e) {
      error.value = e instanceof Error ? e.message : 'Failed to fetch jobs';
    } finally {
      isLoading.value = false;
    }
  };

  const fetchMore = async (): Promise<void> => {
    if (!hasMore.value || isLoading.value) return;

    isLoading.value = true;
    try {
      const response = await jobService.getJobs({
        worldId,
        page: currentPage.value + 1,
        pageSize: pageSize.value,
      });

      jobs.value.push(...response.items);
      currentPage.value++;
    } catch (e) {
      error.value = e instanceof Error ? e.message : 'Failed to fetch more jobs';
    } finally {
      isLoading.value = false;
    }
  };

  const acceptJob = async (jobId: string): Promise<void> => {
    // Implementation
  };

  return {
    jobs,
    totalCount,
    isLoading,
    error,
    hasMore,
    fetchJobs,
    fetchMore,
    acceptJob,
  };
}
```

---

## Testing Standards

### Testing Pyramid

```
         ╱╲
        ╱  ╲         E2E Tests (Selenium)
       ╱    ╲        - Few, slow, expensive
      ╱──────╲       - Critical user journeys only
     ╱        ╲
    ╱          ╲     Integration Tests
   ╱────────────╲    - API endpoints, database
  ╱              ╲   - More than E2E, less than unit
 ╱                ╲
╱──────────────────╲  Unit Tests
                      - Many, fast, cheap
                      - All business logic
```

### XUnit (.NET)

**Test Project Structure:**
```
PilotLife.UnitTests/
├── Services/
│   ├── JobServiceTests.cs
│   ├── ExperienceServiceTests.cs
│   └── AircraftServiceTests.cs
├── Validators/
│   └── JobValidatorTests.cs
├── Mappings/
│   └── JobMappingTests.cs
├── Fixtures/
│   ├── TestFixture.cs
│   └── DatabaseFixture.cs
└── Builders/
    ├── JobBuilder.cs
    └── PlayerBuilder.cs
```

**Test Naming Convention:**
```
MethodName_StateUnderTest_ExpectedBehavior
```

**Test Structure (Arrange-Act-Assert):**
```csharp
public class JobServiceTests
{
    private readonly Mock<IJobRepository> _repositoryMock;
    private readonly Mock<IAuditService> _auditServiceMock;
    private readonly Mock<ILogger<JobService>> _loggerMock;
    private readonly IMapper _mapper;
    private readonly JobService _sut; // System Under Test

    public JobServiceTests()
    {
        _repositoryMock = new Mock<IJobRepository>();
        _auditServiceMock = new Mock<IAuditService>();
        _loggerMock = new Mock<ILogger<JobService>>();
        _mapper = CreateMapper();

        _sut = new JobService(
            _repositoryMock.Object,
            _auditServiceMock.Object,
            _loggerMock.Object,
            _mapper);
    }

    [Fact]
    public async Task CompleteJob_WithValidFlight_ReturnsSuccessAndCreditsPlayer()
    {
        // Arrange
        var job = new JobBuilder()
            .WithStatus(JobStatus.InProgress)
            .WithPayout(12000m)
            .Build();

        var flightData = new FlightDataBuilder()
            .WithDeparture(job.DepartureIcao)
            .WithArrival(job.ArrivalIcao)
            .Build();

        _repositoryMock
            .Setup(r => r.GetByIdAsync(job.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(job);

        // Act
        var result = await _sut.CompleteJobAsync(job.Id, flightData);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(12000m, result.Payout);
        _auditServiceMock.Verify(
            a => a.LogPlayerActionAsync(
                It.IsAny<Guid>(),
                "JOB_COMPLETED",
                It.IsAny<object>(),
                It.IsAny<object>(),
                12000m),
            Times.Once);
    }

    [Fact]
    public async Task CompleteJob_WithWrongArrivalAirport_ReturnsFailure()
    {
        // Arrange
        var job = new JobBuilder()
            .WithArrivalIcao("EGLL")
            .WithStatus(JobStatus.InProgress)
            .Build();

        var flightData = new FlightDataBuilder()
            .WithArrival("EGCC") // Wrong airport
            .Build();

        _repositoryMock
            .Setup(r => r.GetByIdAsync(job.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(job);

        // Act
        var result = await _sut.CompleteJobAsync(job.Id, flightData);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Contains("arrival airport", result.Error, StringComparison.OrdinalIgnoreCase);
    }

    [Theory]
    [InlineData(JobStatus.Available)]
    [InlineData(JobStatus.Completed)]
    [InlineData(JobStatus.Expired)]
    public async Task CompleteJob_WithInvalidStatus_ThrowsInvalidOperationException(JobStatus status)
    {
        // Arrange
        var job = new JobBuilder()
            .WithStatus(status)
            .Build();

        _repositoryMock
            .Setup(r => r.GetByIdAsync(job.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(job);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _sut.CompleteJobAsync(job.Id, new FlightData()));
    }
}
```

**Test Data Builders:**
```csharp
public class JobBuilder
{
    private Guid _id = Guid.NewGuid();
    private Guid _worldId = Guid.NewGuid();
    private string _departureIcao = "EGLL";
    private string _arrivalIcao = "EGCC";
    private decimal _payout = 10000m;
    private JobStatus _status = JobStatus.Available;
    private DateTime _expiresAt = DateTime.UtcNow.AddHours(24);

    public JobBuilder WithId(Guid id)
    {
        _id = id;
        return this;
    }

    public JobBuilder WithDepartureIcao(string icao)
    {
        _departureIcao = icao;
        return this;
    }

    public JobBuilder WithArrivalIcao(string icao)
    {
        _arrivalIcao = icao;
        return this;
    }

    public JobBuilder WithPayout(decimal payout)
    {
        _payout = payout;
        return this;
    }

    public JobBuilder WithStatus(JobStatus status)
    {
        _status = status;
        return this;
    }

    public Job Build()
    {
        return new Job
        {
            Id = _id,
            WorldId = _worldId,
            DepartureIcao = _departureIcao,
            ArrivalIcao = _arrivalIcao,
            Payout = _payout,
            Status = _status,
            ExpiresAt = _expiresAt
        };
    }
}
```

### Integration Tests

```csharp
public class JobsControllerIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public JobsControllerIntegrationTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                // Replace database with in-memory for testing
                var descriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(DbContextOptions<PilotLifeDbContext>));

                if (descriptor != null)
                    services.Remove(descriptor);

                services.AddDbContext<PilotLifeDbContext>(options =>
                {
                    options.UseInMemoryDatabase("TestDb");
                });
            });
        });

        _client = _factory.CreateClient();
    }

    [Fact]
    public async Task GetJobs_ReturnsPagedResults()
    {
        // Arrange
        await SeedTestData();
        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", GetTestToken());

        // Act
        var response = await _client.GetAsync("/api/worlds/test-world/jobs?page=1&pageSize=10");

        // Assert
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadFromJsonAsync<PagedResponse<JobDto>>();

        Assert.NotNull(content);
        Assert.True(content.Items.Count <= 10);
        Assert.True(content.TotalCount >= content.Items.Count);
    }
}
```

### Selenium E2E Tests

**Page Object Model:**
```csharp
// PageObjects/JobBoardPage.cs
public class JobBoardPage
{
    private readonly IWebDriver _driver;
    private readonly WebDriverWait _wait;

    public JobBoardPage(IWebDriver driver)
    {
        _driver = driver;
        _wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
    }

    // Locators
    private By JobCardLocator => By.CssSelector("[data-testid='job-card']");
    private By AcceptButtonLocator => By.CssSelector("[data-testid='accept-job-btn']");
    private By SearchInputLocator => By.CssSelector("[data-testid='search-input']");
    private By LoadingSpinnerLocator => By.CssSelector("[data-testid='loading']");

    // Actions
    public JobBoardPage NavigateTo(string worldId)
    {
        _driver.Navigate().GoToUrl($"{TestConfig.BaseUrl}/worlds/{worldId}/jobs");
        WaitForPageLoad();
        return this;
    }

    public JobBoardPage SearchByDeparture(string icao)
    {
        var searchInput = _wait.Until(d => d.FindElement(SearchInputLocator));
        searchInput.Clear();
        searchInput.SendKeys(icao);
        WaitForPageLoad();
        return this;
    }

    public int GetJobCount()
    {
        return _driver.FindElements(JobCardLocator).Count;
    }

    public JobBoardPage AcceptFirstJob()
    {
        var acceptButton = _wait.Until(d => d.FindElement(AcceptButtonLocator));
        acceptButton.Click();
        return this;
    }

    private void WaitForPageLoad()
    {
        _wait.Until(d =>
        {
            var spinners = d.FindElements(LoadingSpinnerLocator);
            return spinners.Count == 0 || !spinners.Any(s => s.Displayed);
        });
    }
}

// Tests/JobBoardTests.cs
public class JobBoardTests : IDisposable
{
    private readonly IWebDriver _driver;
    private readonly JobBoardPage _jobBoardPage;

    public JobBoardTests()
    {
        var options = new ChromeOptions();
        options.AddArgument("--headless");
        _driver = new ChromeDriver(options);
        _jobBoardPage = new JobBoardPage(_driver);
    }

    [Fact]
    public void JobBoard_SearchByDeparture_FiltersResults()
    {
        // Arrange
        LoginAsTestUser();

        // Act
        _jobBoardPage
            .NavigateTo("test-world")
            .SearchByDeparture("EGLL");

        // Assert
        var jobCount = _jobBoardPage.GetJobCount();
        Assert.True(jobCount > 0);
        // Verify all displayed jobs depart from EGLL
    }

    [Fact]
    public void JobBoard_AcceptJob_ShowsConfirmation()
    {
        // Arrange
        LoginAsTestUser();
        _jobBoardPage.NavigateTo("test-world");

        // Act
        _jobBoardPage.AcceptFirstJob();

        // Assert
        Assert.True(_driver.FindElement(By.CssSelector("[data-testid='success-toast']")).Displayed);
    }

    public void Dispose()
    {
        _driver.Quit();
        _driver.Dispose();
    }
}
```

### Test Coverage Requirements

| Layer | Minimum Coverage | Target Coverage |
|-------|-----------------|-----------------|
| Services (Business Logic) | 80% | 90%+ |
| Validators | 90% | 100% |
| Repositories | 70% | 80% |
| Controllers | 60% | 75% |
| Overall | 75% | 85% |

**Critical paths requiring 100% coverage:**
- Payment processing
- Job completion and payouts
- License validation
- Credit score calculations
- Loan processing

---

## Database & Query Efficiency

### Pagination is MANDATORY

**NEVER retrieve unbounded result sets. ALL list queries MUST be paginated.**

```csharp
// BAD - Returns potentially millions of rows
public async Task<List<Job>> GetAllJobs()
{
    return await _context.Jobs.ToListAsync(); // NEVER DO THIS
}

// GOOD - Paginated with sensible defaults and limits
public async Task<PagedResult<Job>> GetJobsAsync(
    Guid worldId,
    int page = 1,
    int pageSize = 20,
    CancellationToken cancellationToken = default)
{
    // Enforce maximum page size
    pageSize = Math.Min(pageSize, 100);

    var query = _context.Jobs
        .Where(j => j.WorldId == worldId && j.Status == JobStatus.Available);

    var totalCount = await query.CountAsync(cancellationToken);

    var items = await query
        .OrderByDescending(j => j.CreatedAt)
        .Skip((page - 1) * pageSize)
        .Take(pageSize)
        .ToListAsync(cancellationToken);

    return new PagedResult<Job>
    {
        Items = items,
        TotalCount = totalCount,
        Page = page,
        PageSize = pageSize,
        TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
    };
}
```

**Pagination Response Structure:**
```csharp
public class PagedResult<T>
{
    public IReadOnlyList<T> Items { get; init; } = Array.Empty<T>();
    public int TotalCount { get; init; }
    public int Page { get; init; }
    public int PageSize { get; init; }
    public int TotalPages { get; init; }
    public bool HasNextPage => Page < TotalPages;
    public bool HasPreviousPage => Page > 1;
}
```

### Projection - Select Only What You Need

```csharp
// BAD - Loads entire entity with all navigation properties
var jobs = await _context.Jobs
    .Include(j => j.Cargo)
    .Include(j => j.AcceptedBy)
    .Include(j => j.World)
    .ToListAsync();

// GOOD - Project to DTO, select only needed columns
var jobs = await _context.Jobs
    .Where(j => j.WorldId == worldId && j.Status == JobStatus.Available)
    .Select(j => new JobListDto
    {
        Id = j.Id,
        DepartureIcao = j.DepartureIcao,
        ArrivalIcao = j.ArrivalIcao,
        Payout = j.Payout,
        ExpiresAt = j.ExpiresAt,
        CargoDescription = j.Cargo.Name,
        CargoWeightLbs = j.CargoWeightLbs
    })
    .ToListAsync();
```

### Avoid N+1 Queries

```csharp
// BAD - N+1 problem (1 query + N queries for related data)
var jobs = await _context.Jobs.ToListAsync();
foreach (var job in jobs)
{
    var cargo = await _context.CargoTypes.FindAsync(job.CargoTypeId); // N additional queries!
    job.CargoName = cargo.Name;
}

// GOOD - Eager loading when needed
var jobs = await _context.Jobs
    .Include(j => j.CargoType)
    .Where(j => j.WorldId == worldId)
    .ToListAsync();

// BETTER - Projection (no need to load full entity)
var jobs = await _context.Jobs
    .Where(j => j.WorldId == worldId)
    .Select(j => new JobDto
    {
        Id = j.Id,
        CargoName = j.CargoType.Name // EF generates efficient JOIN
    })
    .ToListAsync();
```

### AsNoTracking for Read-Only Queries

```csharp
// For queries that don't modify data, disable change tracking
public async Task<IReadOnlyList<JobDto>> GetAvailableJobsAsync(Guid worldId)
{
    return await _context.Jobs
        .AsNoTracking() // Performance improvement for read-only queries
        .Where(j => j.WorldId == worldId && j.Status == JobStatus.Available)
        .Select(j => new JobDto { /* ... */ })
        .ToListAsync();
}
```

### Batch Operations

```csharp
// BAD - Individual updates in a loop
foreach (var job in expiredJobs)
{
    job.Status = JobStatus.Expired;
    await _context.SaveChangesAsync(); // N database round trips!
}

// GOOD - Batch update
await _context.Jobs
    .Where(j => j.ExpiresAt < DateTime.UtcNow && j.Status == JobStatus.Available)
    .ExecuteUpdateAsync(s => s.SetProperty(j => j.Status, JobStatus.Expired));

// Or with change tracking (single SaveChanges)
var expiredJobs = await _context.Jobs
    .Where(j => j.ExpiresAt < DateTime.UtcNow && j.Status == JobStatus.Available)
    .ToListAsync();

foreach (var job in expiredJobs)
{
    job.Status = JobStatus.Expired;
}

await _context.SaveChangesAsync(); // Single database round trip
```

### Index Strategy

```csharp
// Entity configuration with indexes
public class JobConfiguration : IEntityTypeConfiguration<Job>
{
    public void Configure(EntityTypeBuilder<Job> builder)
    {
        builder.HasKey(j => j.Id);

        // Composite index for common query pattern
        builder.HasIndex(j => new { j.WorldId, j.Status, j.ExpiresAt })
            .HasDatabaseName("IX_Jobs_WorldId_Status_ExpiresAt");

        // Index for foreign key queries
        builder.HasIndex(j => j.AcceptedByPlayerId)
            .HasDatabaseName("IX_Jobs_AcceptedByPlayerId");

        // Index for departure airport searches
        builder.HasIndex(j => new { j.WorldId, j.DepartureIcao, j.Status })
            .HasDatabaseName("IX_Jobs_WorldId_Departure_Status");
    }
}
```

### Query Timeout Policy

```csharp
// Set reasonable timeouts in DbContext
protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
{
    optionsBuilder.UseSqlServer(connectionString, options =>
    {
        options.CommandTimeout(30); // 30 second timeout
        options.EnableRetryOnFailure(3);
    });
}

// For specific long-running queries (reports)
public async Task<ReportData> GenerateMonthlyReportAsync(Guid worldId, CancellationToken ct)
{
    // Use explicit timeout for known long operations
    await using var command = _context.Database.GetDbConnection().CreateCommand();
    command.CommandText = "EXEC GenerateMonthlyReport @WorldId";
    command.CommandTimeout = 300; // 5 minutes for report generation
    // ...
}
```

### Query Guidelines Summary

| Rule | Enforcement |
|------|-------------|
| Always paginate list endpoints | Max 100 items per page |
| Use projections | Select only needed columns |
| AsNoTracking for reads | Default for GET endpoints |
| Avoid N+1 | Use Include or projection |
| Batch updates | Single SaveChanges call |
| Index common queries | Analyze query plans |
| Set timeouts | 30s default, explicit for reports |
| Count before fetch | Show totals in paginated responses |

---

## API Design Standards

### RESTful Conventions

| Action | HTTP Method | URL Pattern | Response |
|--------|-------------|-------------|----------|
| List | GET | `/api/resources` | 200 + PagedResult |
| Get | GET | `/api/resources/{id}` | 200 or 404 |
| Create | POST | `/api/resources` | 201 + Location header |
| Update | PUT | `/api/resources/{id}` | 200 or 204 |
| Partial Update | PATCH | `/api/resources/{id}` | 200 or 204 |
| Delete | DELETE | `/api/resources/{id}` | 204 or 404 |

### URL Structure

```
/api/v1/worlds/{worldId}/jobs                    # Jobs in a world
/api/v1/worlds/{worldId}/jobs/{jobId}            # Specific job
/api/v1/worlds/{worldId}/jobs/{jobId}/accept     # Action on job
/api/v1/worlds/{worldId}/players/me              # Current player
/api/v1/worlds/{worldId}/players/me/aircraft     # Player's aircraft
/api/v1/admin/worlds/{worldId}/submissions       # Admin endpoints
```

### Request/Response Standards

**Successful Response:**
```json
{
  "data": {
    "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "departureIcao": "EGLL",
    "arrivalIcao": "EGCC",
    "payout": 12000
  },
  "meta": {
    "timestamp": "2024-01-15T14:30:00Z",
    "requestId": "abc123"
  }
}
```

**Paginated Response:**
```json
{
  "data": {
    "items": [...],
    "totalCount": 1547,
    "page": 1,
    "pageSize": 20,
    "totalPages": 78,
    "hasNextPage": true,
    "hasPreviousPage": false
  },
  "meta": {
    "timestamp": "2024-01-15T14:30:00Z"
  }
}
```

**Error Response:**
```json
{
  "error": {
    "code": "VALIDATION_ERROR",
    "message": "One or more validation errors occurred",
    "details": [
      {
        "field": "departureIcao",
        "message": "Departure ICAO is required"
      },
      {
        "field": "payout",
        "message": "Payout must be greater than 0"
      }
    ]
  },
  "meta": {
    "timestamp": "2024-01-15T14:30:00Z",
    "requestId": "abc123"
  }
}
```

### Validation with FluentValidation

```csharp
public class AcceptJobRequestValidator : AbstractValidator<AcceptJobRequest>
{
    public AcceptJobRequestValidator()
    {
        RuleFor(x => x.JobId)
            .NotEmpty()
            .WithMessage("Job ID is required");

        RuleFor(x => x.RentalAircraftId)
            .NotEmpty()
            .When(x => x.UseRental)
            .WithMessage("Rental aircraft ID is required when using rental");
    }
}

// Register in DI
services.AddValidatorsFromAssemblyContaining<AcceptJobRequestValidator>();

// Controller usage with validation filter
[HttpPost("{jobId}/accept")]
public async Task<IActionResult> AcceptJob(
    Guid jobId,
    [FromBody] AcceptJobRequest request)
{
    // Validation happens automatically via filter
    var result = await _jobService.AcceptJobAsync(jobId, request);
    return Ok(result);
}
```

### API Versioning

```csharp
// Program.cs
builder.Services.AddApiVersioning(options =>
{
    options.DefaultApiVersion = new ApiVersion(1, 0);
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.ReportApiVersions = true;
    options.ApiVersionReader = ApiVersionReader.Combine(
        new UrlSegmentApiVersionReader(),
        new HeaderApiVersionReader("X-Api-Version"));
});

// Controller
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
public class JobsController : ControllerBase
{
    // ...
}
```

---

## Error Handling & Logging

### Exception Hierarchy

```csharp
// Base application exception
public abstract class PilotLifeException : Exception
{
    public string Code { get; }
    public int StatusCode { get; }

    protected PilotLifeException(string code, string message, int statusCode = 500)
        : base(message)
    {
        Code = code;
        StatusCode = statusCode;
    }
}

// Specific exceptions
public class NotFoundException : PilotLifeException
{
    public NotFoundException(string resource, Guid id)
        : base("NOT_FOUND", $"{resource} with ID {id} was not found", 404)
    { }
}

public class ValidationException : PilotLifeException
{
    public IReadOnlyList<ValidationError> Errors { get; }

    public ValidationException(IEnumerable<ValidationError> errors)
        : base("VALIDATION_ERROR", "One or more validation errors occurred", 400)
    {
        Errors = errors.ToList();
    }
}

public class BusinessRuleException : PilotLifeException
{
    public BusinessRuleException(string code, string message)
        : base(code, message, 422)
    { }
}

public class UnauthorizedException : PilotLifeException
{
    public UnauthorizedException(string message = "Unauthorized")
        : base("UNAUTHORIZED", message, 401)
    { }
}
```

### Global Exception Handler

```csharp
public class GlobalExceptionHandler : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger;

    public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
    {
        _logger = logger;
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext context,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var (statusCode, response) = exception switch
        {
            PilotLifeException ex => (ex.StatusCode, CreateErrorResponse(ex)),
            _ => (500, CreateGenericErrorResponse(exception))
        };

        // Log based on severity
        if (statusCode >= 500)
        {
            _logger.LogError(exception, "Unhandled exception: {Message}", exception.Message);
        }
        else
        {
            _logger.LogWarning("Application exception: {Code} - {Message}",
                response.Error.Code, response.Error.Message);
        }

        context.Response.StatusCode = statusCode;
        await context.Response.WriteAsJsonAsync(response, cancellationToken);

        return true;
    }

    private static ErrorResponse CreateErrorResponse(PilotLifeException ex)
    {
        return new ErrorResponse
        {
            Error = new ErrorDetails
            {
                Code = ex.Code,
                Message = ex.Message,
                Details = ex is ValidationException ve ? ve.Errors : null
            }
        };
    }

    private static ErrorResponse CreateGenericErrorResponse(Exception ex)
    {
        return new ErrorResponse
        {
            Error = new ErrorDetails
            {
                Code = "INTERNAL_ERROR",
                Message = "An unexpected error occurred" // Don't leak internal details
            }
        };
    }
}
```

### Structured Logging

```csharp
public class JobService : IJobService
{
    private readonly ILogger<JobService> _logger;

    public async Task<JobCompletionResult> CompleteJobAsync(Guid jobId, FlightData data)
    {
        using var scope = _logger.BeginScope(new Dictionary<string, object>
        {
            ["JobId"] = jobId,
            ["PlayerId"] = data.PlayerId,
            ["Operation"] = "CompleteJob"
        });

        _logger.LogInformation("Starting job completion");

        try
        {
            var job = await _repository.GetByIdAsync(jobId);

            if (job is null)
            {
                _logger.LogWarning("Job not found for completion");
                throw new NotFoundException("Job", jobId);
            }

            // Process completion...

            _logger.LogInformation(
                "Job completed successfully. Payout: {Payout}, XP: {XPAwarded}",
                result.Payout,
                result.XPAwarded);

            return result;
        }
        catch (Exception ex) when (ex is not PilotLifeException)
        {
            _logger.LogError(ex, "Unexpected error during job completion");
            throw;
        }
    }
}
```

### Logging Standards

| Level | When to Use | Example |
|-------|-------------|---------|
| Trace | Detailed diagnostic info | Query parameters, method entry/exit |
| Debug | Development diagnostics | Variable values, state changes |
| Information | Normal operations | Job completed, user logged in |
| Warning | Recoverable issues | Validation failed, retry attempt |
| Error | Unrecoverable errors | Exception caught, operation failed |
| Critical | System failures | Database down, out of memory |

**What to Log:**
- Request/response correlation IDs
- User/player context
- Operation being performed
- Timing for slow operations
- Business events (job completed, payment processed)

**What NOT to Log:**
- Passwords or secrets
- Full credit card numbers
- Personal identifying information (PII)
- Large payloads (log summary instead)

---

## Security Standards

### Authentication & Authorization

```csharp
// JWT configuration
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = configuration["Jwt:Issuer"],
            ValidAudience = configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(configuration["Jwt:Key"]!))
        };
    });

// Authorization policies
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("RequireAdmin", policy =>
        policy.RequireRole("Admin", "SuperAdmin"));

    options.AddPolicy("RequireWorldAccess", policy =>
        policy.Requirements.Add(new WorldAccessRequirement()));
});
```

### Input Validation

**Always validate and sanitize input:**

```csharp
public class CreateJobRequestValidator : AbstractValidator<CreateJobRequest>
{
    public CreateJobRequestValidator()
    {
        RuleFor(x => x.DepartureIcao)
            .NotEmpty()
            .Length(4)
            .Matches("^[A-Z0-9]{4}$")
            .WithMessage("Invalid ICAO code format");

        RuleFor(x => x.Payout)
            .GreaterThan(0)
            .LessThanOrEqualTo(10_000_000);

        RuleFor(x => x.CargoWeightLbs)
            .GreaterThan(0)
            .LessThanOrEqualTo(500_000);
    }
}
```

### SQL Injection Prevention

**Always use parameterized queries:**

```csharp
// BAD - SQL injection vulnerability
var query = $"SELECT * FROM Jobs WHERE DepartureIcao = '{userInput}'";

// GOOD - Parameterized via EF Core
var jobs = await _context.Jobs
    .Where(j => j.DepartureIcao == userInput)
    .ToListAsync();

// GOOD - Parameterized raw SQL
var jobs = await _context.Jobs
    .FromSqlInterpolated($"SELECT * FROM Jobs WHERE DepartureIcao = {userInput}")
    .ToListAsync();
```

### Secrets Management

**Never hardcode secrets:**

```csharp
// BAD
var connectionString = "Server=prod;Database=PilotLife;User=admin;Password=secret123";

// GOOD - Use configuration
var connectionString = configuration.GetConnectionString("DefaultConnection");

// GOOD - Use Azure Key Vault, AWS Secrets Manager, etc.
builder.Configuration.AddAzureKeyVault(
    new Uri($"https://{vaultName}.vault.azure.net/"),
    new DefaultAzureCredential());
```

---

## Code Review & Git Workflow

### Branch Naming

```
feature/PLF-123-add-job-completion
bugfix/PLF-456-fix-payout-calculation
hotfix/PLF-789-critical-auth-fix
refactor/PLF-012-extract-validation-service
docs/PLF-345-update-api-documentation
```

### Commit Messages

```
<type>(<scope>): <subject>

<body>

<footer>
```

**Types:**
- `feat`: New feature
- `fix`: Bug fix
- `docs`: Documentation
- `style`: Formatting (no code change)
- `refactor`: Code refactoring
- `test`: Adding tests
- `chore`: Maintenance

**Examples:**
```
feat(jobs): add quick job rental system

Implement rental aircraft selection for quick jobs.
Players can now take jobs using rented aircraft from
system fleet or other players.

Closes PLF-123
```

```
fix(banking): correct interest calculation for starter loans

Interest was being calculated on original balance instead
of remaining balance. Updated to use amortization formula.

Fixes PLF-456
```

### Pull Request Requirements

**Before submitting a PR:**

- [ ] All tests pass locally
- [ ] New code has tests (maintain coverage)
- [ ] No compiler warnings
- [ ] Code follows style guidelines
- [ ] API changes documented
- [ ] Database migrations tested
- [ ] Self-reviewed the diff

**PR Description Template:**
```markdown
## Summary
Brief description of changes

## Changes
- Added X
- Modified Y
- Removed Z

## Testing
- [ ] Unit tests added/updated
- [ ] Integration tests added/updated
- [ ] Manual testing performed

## Screenshots (if UI changes)

## Breaking Changes
List any breaking changes

## Related Issues
Closes #123
```

### Code Review Checklist

**Reviewer responsibilities:**

- [ ] Code is readable and maintainable
- [ ] Business logic is correct
- [ ] Error handling is appropriate
- [ ] Security considerations addressed
- [ ] Performance implications considered
- [ ] Tests are meaningful and sufficient
- [ ] No code smells or anti-patterns
- [ ] Follows project conventions

**Review comments should be:**
- Constructive and respectful
- Specific with suggestions
- Distinguishing between blocking vs. non-blocking

---

## Summary

| Principle | Enforcement |
|-----------|-------------|
| TDD | Write tests first, always |
| Interface-based | All dependencies via interfaces |
| Loose coupling | DI for everything |
| Pagination | Max 100 items, always paginate |
| Projections | Select only needed columns |
| Async | All I/O operations async |
| DateTimeOffset | Always use instead of DateTime for timestamps |
| Validation | FluentValidation on all inputs |
| Error handling | Global handler, typed exceptions |
| Logging | Structured, correlated, appropriate level |
| Testing | XUnit (unit/integration), Selenium (E2E) |
| Coverage | 75% minimum, 85% target |
| Security | Never trust input, parameterize queries |

**Remember:** Code is read more often than written. Prioritize clarity, testability, and maintainability over cleverness.
