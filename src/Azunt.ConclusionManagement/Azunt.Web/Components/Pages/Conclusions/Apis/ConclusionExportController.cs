using Azunt.ConclusionManagement;
using Microsoft.AspNetCore.Mvc;

namespace Azunt.Web.Components.Pages.Conclusions.Apis;

[Route("api/[controller]")]
[ApiController]
public class ConclusionExportController : ControllerBase
{
    private readonly IConclusionRepository _repository;

    public ConclusionExportController(IConclusionRepository repository)
    {
        _repository = repository;
    }

    /// <summary>
    /// Conclusions 목록을 Microsoft Open XML SDK 기반 Excel 파일로 다운로드합니다.
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
}
