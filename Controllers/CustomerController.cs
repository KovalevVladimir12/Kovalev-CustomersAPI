using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Kovalev.Data;
using Kovalev.Models;

namespace Kovalev.Controllers;

[Authorize]
[Route("api/[controller]")]
[ApiController]
public class CustomerController : ControllerBase
{
    private readonly AppDbContext _context;

    public CustomerController(AppDbContext context)
    {
        _context = context;
    }

    // GET: api/customer
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Customer>>> GetCustomers()
    {
        try
        {
            var customers = await _context.Customers.ToListAsync();
            return Ok(customers);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = ex.Message });
        }
    }

    // GET: api/customer/5
    [HttpGet("{id}")]
    public async Task<ActionResult<Customer>> GetCustomer(int id)
    {
        try
        {
            var customer = await _context.Customers.FindAsync(id);

            if (customer == null)
            {
                return NotFound(new { message = "Клиент не найден" });
            }

            return Ok(customer);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = ex.Message });
        }
    }

    // POST: api/customer
    [HttpPost]
    public async Task<ActionResult<Customer>> CreateCustomer(Customer customer)
    {
        try
        {
            // Проверяем обязательные поля
            if (string.IsNullOrEmpty(customer.FirstName))
            {
                return BadRequest(new { message = "Имя обязательно для заполнения" });
            }

            if (string.IsNullOrEmpty(customer.Email))
            {
                return BadRequest(new { message = "Email обязателен для заполнения" });
            }

            // Устанавливаем ID = 0 для автоинкремента
            customer.CustomerId = 0;

            _context.Customers.Add(customer);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetCustomer), new { id = customer.CustomerId }, customer);
        }
        catch (DbUpdateException ex)
        {
            // Получаем внутреннее исключение
            var innerException = ex.InnerException;
            var errorMessage = "Ошибка базы данных: ";

            if (innerException != null)
            {
                errorMessage += innerException.Message;

                // Для PostgreSQL покажем более детально
                if (innerException is Npgsql.PostgresException pgEx)
                {
                    errorMessage = $"PostgreSQL Error: {pgEx.MessageText} (Code: {pgEx.SqlState})";
                }
            }
            else
            {
                errorMessage += ex.Message;
            }

            return StatusCode(500, new { message = errorMessage });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = ex.Message });
        }
    }

    // PUT: api/customer/5
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateCustomer(int id, Customer customer)
    {
        try
        {
            if (id != customer.CustomerId)
            {
                return BadRequest(new { message = "ID в URL не совпадает с ID клиента" });
            }

            var existingCustomer = await _context.Customers.FindAsync(id);
            if (existingCustomer == null)
            {
                return NotFound(new { message = "Клиент не найден" });
            }

            // Обновляем поля
            existingCustomer.FirstName = customer.FirstName;
            existingCustomer.Email = customer.Email;
            existingCustomer.Address = customer.Address;

            await _context.SaveChangesAsync();

            return Ok(new { message = "Клиент успешно обновлен" });
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!CustomerExists(id))
            {
                return NotFound(new { message = "Клиент не найден" });
            }
            else
            {
                throw;
            }
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = ex.Message });
        }
    }

    // DELETE: api/customer/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteCustomer(int id)
    {
        try
        {
            var customer = await _context.Customers.FindAsync(id);
            if (customer == null)
            {
                return NotFound(new { message = "Клиент не найден" });
            }

            _context.Customers.Remove(customer);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Клиент успешно удален" });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = ex.Message });
        }
    }

    private bool CustomerExists(int id)
    {
        return _context.Customers.Any(e => e.CustomerId == id);
    }
}