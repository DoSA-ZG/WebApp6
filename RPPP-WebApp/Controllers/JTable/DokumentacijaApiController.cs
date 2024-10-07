using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RPPP_WebApp.Data;
using RPPP_WebApp.Models;

[Route("api/Dokumentacija")]
[ApiController]
public class DokumentacijaApiController : ControllerBase
{
    private readonly RPPP06Context _context;

    public DokumentacijaApiController(RPPP06Context context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Dokumentacija>>> GetDokumentacije()
    {
        return await _context.Dokumentacija.ToListAsync();
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Dokumentacija>> GetDokumentacija(int id)
    {
        var dokumentacija = await _context.Dokumentacija.FindAsync(id);

        if (dokumentacija == null)
        {
            return NotFound();
        }

        return dokumentacija;
    }

    [HttpPost]
    public async Task<ActionResult<Dokumentacija>> PostDokumentacija(Dokumentacija dokumentacija)
    {
        _context.Dokumentacija.Add(dokumentacija);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetDokumentacija), new { id = dokumentacija.DokumentacijaId }, dokumentacija);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> PutDokumentacija(int id, Dokumentacija dokumentacija)
    {
        if (id != dokumentacija.DokumentacijaId)
        {
            return BadRequest();
        }

        _context.Entry(dokumentacija).State = EntityState.Modified;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!DokumentacijaExists(id))
            {
                return NotFound();
            }
            else
            {
                throw;
            }
        }

        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteDokumentacija(int id)
    {
        var dokumentacija = await _context.Dokumentacija.FindAsync(id);
        if (dokumentacija == null)
        {
            return NotFound();
        }

        _context.Dokumentacija.Remove(dokumentacija);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    private bool DokumentacijaExists(int id)
    {
        return _context.Dokumentacija.Any(e => e.DokumentacijaId == id);
    }
}
