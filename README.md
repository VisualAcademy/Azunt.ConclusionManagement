# Azunt.ConclusionManagement

Azunt.ConclusionManagement는 `Conclusions` 테이블을 기준으로 결론 이름(`Name`)과 상세 내용(`Content`)을 관리하는 재사용 가능한 .NET 모듈입니다.

이 리포지토리는 NuGet 패키지로 게시할 클래스 라이브러리인 `Azunt.ConclusionManagement`와, 해당 패키지를 실제로 테스트할 수 있는 Blazor Server 프로젝트인 `Azunt.Web`을 함께 포함합니다.

## 포함 프로젝트

```output
src/Azunt.ConclusionManagement
│  Azunt.ConclusionManagement.sln
│
├─Azunt.ConclusionManagement
│      Azunt.ConclusionManagement.csproj
│
├─Azunt.SqlServer
│      Azunt.SqlServer.sqlproj
│
└─Azunt.Web
       Azunt.Web.csproj
```

## 주요 특징

- `Conclusions` 테이블에 `Content NVARCHAR(MAX) NULL` 컬럼을 포함합니다.
- `Azunt.Web`은 EF Core In-Memory DB를 사용하므로 SQL Server 데이터베이스 프로젝트를 게시하지 않아도 `/Conclusions` 페이지에서 CRUD를 바로 테스트할 수 있습니다.
- `Dul.dll`과 `DulPager`를 사용하지 않습니다.
- `ArticleSet`과 `FilterOptions`는 `Azunt.ConclusionManagement` 패키지 내부의 경량 타입을 사용하므로 `Dul.dll` 없이 컴파일됩니다.
- 페이징 UI는 `Azunt.Components` NuGet 패키지의 `Azunt.Components.Paging.Pager` 컴포넌트를 사용합니다.
- 삭제 확인 창은 Azunt.Web 내부 인라인 모달로 구현하여 `Del` 버튼 클릭 후 삭제와 목록 갱신이 바로 동작하도록 구성했습니다.
- Excel 다운로드는 `EPPlus`가 아니라 Microsoft Open XML SDK(`DocumentFormat.OpenXml`) 기반의 `ConclusionExcelExporter`를 사용합니다.
- 운영/배포 시에는 EF Core SQL Server, Dapper, ADO.NET 방식도 선택할 수 있습니다.

## 빠른 실행

Visual Studio에서 다음 솔루션을 엽니다.

```output
src/Azunt.ConclusionManagement/Azunt.ConclusionManagement.sln
```

시작 프로젝트를 `Azunt.Web`으로 설정한 뒤 실행합니다.

실행 후 다음 URL로 이동합니다.

```output
/Conclusions
```

`Azunt.Web`은 다음 코드로 Conclusion 모듈을 In-Memory 모드로 등록합니다.

```csharp
builder.Services.AddDependencyInjectionContainerForConclusionApp(
    mode: ConclusionServicesRegistrationExtensions.RepositoryMode.EfCoreInMemory);
```

따라서 SQL Server 데이터베이스 프로젝트를 게시하지 않아도 Create, Read, Update, Delete, Search, Sort, Paging, Excel Export를 바로 테스트할 수 있습니다.

## NuGet 패키지 생성

클래스 라이브러리 패키지는 다음 명령으로 생성할 수 있습니다.

```bash
cd src/Azunt.ConclusionManagement
dotnet pack Azunt.ConclusionManagement/Azunt.ConclusionManagement.csproj -c Release
```

생성된 `.nupkg` 파일을 NuGet 서버에 게시한 뒤, 실제 Azunt.Web 프로젝트에서 패키지 참조 방식으로 테스트하면 됩니다.

## SQL Server 사용 예

운영 환경에서 SQL Server를 사용하려면 다음처럼 등록합니다.

```csharp
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDependencyInjectionContainerForConclusionApp(
    connectionString,
    ConclusionServicesRegistrationExtensions.RepositoryMode.EfCoreSqlServer);
```

Dapper 또는 ADO.NET 모드도 같은 연결 문자열을 전달하여 사용할 수 있습니다.

## 이번 테스트 프로젝트 구성 메모

- `Azunt.Web.Client` 프로젝트는 포함하지 않습니다.
- Blazor Server의 `InteractiveServer` 렌더링만 사용합니다.
- WebAssembly/Webcil 변환 경로에서 발생할 수 있는 `tmp-webcil` 관련 오류를 피하기 위해 `Microsoft.AspNetCore.Components.WebAssembly.Server` 참조와 `AddInteractiveWebAssemblyComponents()` 설정을 제거했습니다.
- `Azunt.Web`은 `Azunt.ConclusionManagement` 프로젝트를 직접 참조하여 NuGet 게시 전 로컬 테스트가 가능하고, 최종 게시 후에는 Project Reference를 Package Reference로 교체해 테스트할 수 있습니다.
