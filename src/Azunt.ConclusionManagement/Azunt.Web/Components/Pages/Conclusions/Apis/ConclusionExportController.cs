using Azunt.ConclusionManagement;
using Azunt.Web.Services;
using Microsoft.AspNetCore.Mvc;

namespace Azunt.Web.Components.Pages.Conclusions.Apis;

[Route("api/[controller]")]
[ApiController]
public class ConclusionExportController : ControllerBase
{
    private readonly IConclusionRepository _repository;
    private readonly IUserService _userService;

    public ConclusionExportController(IConclusionRepository repository, IUserService userService)
    {
        _repository = repository;
        _userService = userService;
    }

    /// <summary>
    /// 단일 테넌트(DefaultConnection 또는 기본 In-Memory) Conclusions 목록을 Excel 파일로 다운로드합니다.
    /// GET /api/ConclusionExport/Excel
    /// </summary>
    [HttpGet("Excel")]
    public async Task<IActionResult> ExportToExcel()
    {
        var items = await _repository.GetAllAsync();
        var bytes = ConclusionExcelExporter.ExportToExcel(items);
        var fileName = $"{DateTime.Now:yyyyMMddHHmmss}_Conclusions.xlsx";

        return File(
            bytes,
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            fileName);
    }

    /// <summary>
    /// 현재 사용자의 테넌트 연결 문자열을 사용하여 Conclusions 목록을 Excel 파일로 다운로드합니다.
    /// GET /api/ConclusionExport/ExcelByTenant
    /// </summary>
    [HttpGet("ExcelByTenant")]
    public async Task<IActionResult> ExportToExcelByTenant()
    {
        var user = _userService.GetUserNotCached();
        var connectionString = user.Tenant.ConnectionString;
        var items = await _repository.GetAllAsync(connectionString);
        var bytes = ConclusionExcelExporter.ExportToExcel(items, "Tenant Conclusions");
        var fileName = $"{DateTime.Now:yyyyMMddHHmmss}_Tenant_Conclusions.xlsx";

        return File(
            bytes,
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            fileName);
    }
}
