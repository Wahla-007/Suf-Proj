# Developer's Guide

## For Developers Extending This Project

---

## 🏗️ Project Architecture

### MVC Pattern
```
Model (Data)
  ↓
Controller (Logic)
  ↓
View (UI)
```

### Our Implementation
```
Models/
├─ AspNetUser.cs           ← User data
├─ TeacherAttendance.cs    ← Attendance data
├─ WeeklyMenu.cs           ← Menu data
├─ MonthlyBill.cs          ← Billing data
├─ AppDbContext.cs         ← Database context
└─ ErrorViewModel.cs       ← Error handling

    ↓

Controllers/
├─ HomeController.cs           ← Routes: /Home
├─ AspNetUserController.cs     ← Routes: /AspNetUser
├─ TeacherController.cs        ← Routes: /Teacher
├─ WeeklyMenuController.cs     ← Routes: /WeeklyMenu
├─ TeacherAttendanceController.cs ← Routes: /TeacherAttendance
└─ MonthlyBillController.cs    ← Routes: /MonthlyBill

    ↓

Views/
├─ Home/
│  ├─ Index.cshtml
│  └─ Privacy.cshtml
├─ Teacher/
│  ├─ Dashboard.cshtml
│  ├─ AttendanceHistory.cshtml
│  ├─ MyBills.cshtml
│  └─ ViewMenu.cshtml
├─ AspNetUser/, WeeklyMenu/, TeacherAttendance/, MonthlyBill/
│  └─ Create.cshtml, Edit.cshtml, Delete.cshtml, Details.cshtml, Index.cshtml
└─ Shared/
   └─ _Layout.cshtml
```

---

## 🗄️ Database Context

Located in `Models/AppDbContext.cs`:

```csharp
public class AppDbContext : DbContext
{
    public DbSet<AspNetUser> AspNetUsers { get; set; }
    public DbSet<TeacherAttendance> TeacherAttendances { get; set; }
    public DbSet<WeeklyMenu> WeeklyMenus { get; set; }
    public DbSet<MonthlyBill> MonthlyBills { get; set; }
    
    // Connection string configured here
}
```

**To access database in a controller**:
```csharp
private readonly AppDbContext _context;

public MyController(AppDbContext context)
{
    _context = context;
}

// Then use:
var users = await _context.AspNetUsers.ToListAsync();
```

---

## 🎮 Controller Pattern

### Standard CRUD Controller (Example)
```csharp
public class TeacherController : Controller
{
    private readonly AppDbContext _context;

    public TeacherController(AppDbContext context)
    {
        _context = context;
    }

    // GET - Display list
    public async Task<IActionResult> Index()
    {
        var data = await _context.TeacherAttendances.ToListAsync();
        return View(data);
    }

    // GET - Display create form
    public IActionResult Create()
    {
        return View();
    }

    // POST - Process form submission
    [HttpPost]
    public async Task<IActionResult> Create(TeacherAttendance model)
    {
        if (ModelState.IsValid)
        {
            _context.Add(model);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        return View(model);
    }

    // GET - Display single record
    public async Task<IActionResult> Details(int id)
    {
        var item = await _context.TeacherAttendances.FindAsync(id);
        if (item == null) return NotFound();
        return View(item);
    }

    // GET - Display edit form
    public async Task<IActionResult> Edit(int id)
    {
        var item = await _context.TeacherAttendances.FindAsync(id);
        if (item == null) return NotFound();
        return View(item);
    }

    // POST - Process edit
    [HttpPost]
    public async Task<IActionResult> Edit(int id, TeacherAttendance model)
    {
        if (id != model.Id) return NotFound();
        
        if (ModelState.IsValid)
        {
            try
            {
                _context.Update(model);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ItemExists(model.Id)) return NotFound();
                throw;
            }
        }
        return View(model);
    }

    // GET - Display delete form
    public async Task<IActionResult> Delete(int id)
    {
        var item = await _context.TeacherAttendances.FindAsync(id);
        if (item == null) return NotFound();
        return View(item);
    }

    // POST - Process delete
    [HttpPost, ActionName("Delete")]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var item = await _context.TeacherAttendances.FindAsync(id);
        if (item != null)
        {
            _context.TeacherAttendances.Remove(item);
            await _context.SaveChangesAsync();
        }
        return RedirectToAction(nameof(Index));
    }

    private bool ItemExists(int id)
    {
        return _context.TeacherAttendances.Any(e => e.Id == id);
    }
}
```

---

## 📝 Razor View Basics

### Display Data
```cshtml
@model TeacherAttendance

<h1>@Model.Id</h1>
<p>Date: @Model.Date?.ToString("dd-MM-yyyy")</p>
```

### Forms
```cshtml
<form asp-action="Create">
    <div>
        <label asp-for="TeacherId"></label>
        <input asp-for="TeacherId" />
    </div>
    
    <div>
        <label asp-for="Date"></label>
        <input asp-for="Date" type="date" />
    </div>
    
    <button type="submit">Save</button>
</form>
```

### Loops
```cshtml
@foreach (var item in Model)
{
    <tr>
        <td>@item.Date</td>
        <td>
            @if (item.IsVerified == true)
            {
                <span class="badge bg-success">Verified</span>
            }
        </td>
    </tr>
}
```

### Conditionals
```cshtml
@if (condition)
{
    <p>True condition</p>
}
else if (other_condition)
{
    <p>Other condition</p>
}
else
{
    <p>Default</p>
}
```

### Links & Navigation
```cshtml
<!-- Link to action -->
<a asp-action="Edit" asp-route-id="@item.Id">Edit</a>

<!-- Link to controller -->
<a asp-controller="Home" asp-action="Index">Home</a>

<!-- URL generation -->
@Url.Action("Edit", "Teacher", new { id = 5 })
```

---

## 🔧 Adding a New Feature

### Step 1: Create Model
```csharp
// Models/PaymentRecord.cs
public class PaymentRecord
{
    public int Id { get; set; }
    public string TeacherId { get; set; }
    public DateTime PaymentDate { get; set; }
    public decimal Amount { get; set; }
    
    public virtual AspNetUser Teacher { get; set; }
}
```

### Step 2: Add to DbContext
```csharp
// Models/AppDbContext.cs
public DbSet<PaymentRecord> PaymentRecords { get; set; }
```

### Step 3: Create Controller
```csharp
// Controllers/PaymentController.cs
[ApiController]
[Route("api/[controller]")]
public class PaymentController : Controller
{
    private readonly AppDbContext _context;

    public PaymentController(AppDbContext context)
    {
        _context = context;
    }

    // Add CRUD actions here
}
```

### Step 4: Create Views
- `Views/Payment/Index.cshtml`
- `Views/Payment/Create.cshtml`
- `Views/Payment/Edit.cshtml`
- `Views/Payment/Delete.cshtml`

### Step 5: Update Navigation
```cshtml
<!-- Views/Shared/_Layout.cshtml -->
<li class="nav-item">
    <a class="nav-link" asp-controller="Payment" asp-action="Index">Payments</a>
</li>
```

---

## 🚀 Common Extensions

### Add Authentication
```csharp
// Program.cs
builder.Services
    .AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = true)
    .AddEntityFrameworkStores<AppDbContext>();
```

### Add API Endpoint
```csharp
[ApiController]
[Route("api/[controller]")]
public class AttendanceApiController : ControllerBase
{
    private readonly AppDbContext _context;

    [HttpGet("{id}")]
    public async Task<IActionResult> GetAttendance(int id)
    {
        var record = await _context.TeacherAttendances.FindAsync(id);
        if (record == null) return NotFound();
        return Ok(record);
    }
}
```

### Add Logging
```csharp
private readonly ILogger<TeacherController> _logger;

public TeacherController(AppDbContext context, ILogger<TeacherController> logger)
{
    _context = context;
    _logger = logger;
}

// Usage
_logger.LogInformation("Teacher accessed dashboard");
```

### Add Validation
```csharp
[Display(Name = "Breakfast Rate")]
[Range(0.01, 1000, ErrorMessage = "Rate must be between 0.01 and 1000")]
public decimal BreakfastRate { get; set; }
```

---

## 🧪 Testing Patterns

### Unit Test Example
```csharp
[TestClass]
public class TeacherAttendanceTests
{
    private AppDbContext _context;

    [TestInitialize]
    public void Setup()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase("TestDb")
            .Options;
        _context = new AppDbContext(options);
    }

    [TestMethod]
    public async Task CanCreateAttendance()
    {
        // Arrange
        var attendance = new TeacherAttendance { TeacherId = "T1", Date = DateOnly.FromDateTime(DateTime.Now) };
        
        // Act
        _context.Add(attendance);
        await _context.SaveChangesAsync();
        
        // Assert
        Assert.AreEqual(1, _context.TeacherAttendances.Count());
    }
}
```

---

## 📦 NuGet Packages

Current packages in `mess_management.csproj`:
```xml
<PackageReference Include="Microsoft.EntityFrameworkCore" Version="8.x" />
<PackageReference Include="Microsoft.EntityFrameworkCore.SqlServer" Version="8.x" />
<PackageReference Include="Microsoft.EntityFrameworkCore.Tools" Version="8.x" />
```

To add new package:
```bash
dotnet add package PackageName
```

---

## 🔌 Useful EF Core Queries

### Filter
```csharp
var records = await _context.TeacherAttendances
    .Where(r => r.Date > DateTime.Now.AddDays(-30))
    .ToListAsync();
```

### Order
```csharp
var records = await _context.TeacherAttendances
    .OrderByDescending(r => r.Date)
    .ToListAsync();
```

### Join/Include
```csharp
var bills = await _context.MonthlyBills
    .Include(b => b.Teacher)
    .Where(b => b.IsPaid == false)
    .ToListAsync();
```

### Group
```csharp
var summary = await _context.TeacherAttendances
    .GroupBy(r => r.TeacherId)
    .Select(g => new { TeacherId = g.Key, Count = g.Count() })
    .ToListAsync();
```

### Aggregate
```csharp
var total = await _context.MonthlyBills
    .Where(b => b.IsPaid == false)
    .SumAsync(b => b.TotalAmount ?? 0);
```

---

## 🎨 Bootstrap Classes Reference

### Buttons
```html
<button class="btn btn-primary">Primary</button>
<button class="btn btn-success">Success</button>
<button class="btn btn-danger">Danger</button>
<button class="btn btn-warning">Warning</button>
<button class="btn btn-info">Info</button>
```

### Cards
```html
<div class="card">
    <div class="card-header bg-primary text-white">
        <h5 class="card-title">Title</h5>
    </div>
    <div class="card-body">
        <p class="card-text">Content</p>
    </div>
    <div class="card-footer">
        Footer
    </div>
</div>
```

### Grid
```html
<div class="row">
    <div class="col-md-6">Half width</div>
    <div class="col-md-6">Half width</div>
</div>
```

### Badges & Alerts
```html
<span class="badge bg-success">Verified</span>
<div class="alert alert-warning">Warning message</div>
<div class="alert alert-danger">Error message</div>
```

---

## 🐛 Debugging Tips

### Enable Logging
```csharp
// Program.cs
builder.Services.AddLogging();
builder.Logging.AddConsole();
```

### Check Model State
```csharp
if (!ModelState.IsValid)
{
    var errors = ModelState.Values.SelectMany(v => v.Errors);
    foreach (var error in errors)
    {
        Console.WriteLine(error.ErrorMessage);
    }
}
```

### SQL Query Logging
```csharp
optionsBuilder.LogTo(Console.WriteLine);
```

---

## 📚 Resources

- [Microsoft Docs - ASP.NET Core](https://docs.microsoft.com/en-us/aspnet/core/)
- [Entity Framework Core Docs](https://docs.microsoft.com/en-us/ef/core/)
- [Bootstrap Documentation](https://getbootstrap.com/docs/)
- [C# Language Guide](https://docs.microsoft.com/en-us/dotnet/csharp/)

---

## 🎯 Best Practices

1. **Separation of Concerns**
   - Models: Data layer
   - Controllers: Logic layer
   - Views: Presentation layer

2. **Dependency Injection**
   - Always inject dependencies in constructor
   - Use interfaces for testability

3. **Async/Await**
   - Use async operations for database calls
   - Always use `await` with `ToListAsync()`

4. **Error Handling**
   - Try-catch for database operations
   - Validate user input
   - Return appropriate HTTP status codes

5. **Security**
   - Validate all input
   - Use parameterized queries (EF Core does this)
   - Implement authorization checks

6. **Performance**
   - Use indexes on frequently queried columns
   - Use `Include()` to avoid N+1 queries
   - Implement pagination for large datasets

---

## 📝 Code Style

### Naming Conventions
```csharp
// Classes - PascalCase
public class TeacherAttendance { }

// Methods - PascalCase
public async Task<IActionResult> GetAttendance() { }

// Variables - camelCase
var attendanceRecord = new TeacherAttendance();
private readonly AppDbContext _context;

// Constants - UPPER_CASE
private const int MAX_RECORDS = 100;
```

### Comments
```csharp
// Single line comment for simple statements

/// <summary>
/// XML documentation for public methods
/// </summary>
/// <param name="id">The record ID</param>
/// <returns>The attendance record</returns>
public async Task<TeacherAttendance> GetAttendance(int id) { }

/* Multi-line comment
   for complex logic
   explanation */
```

---

## 🚀 Deployment Checklist

- [ ] All tests passing
- [ ] No compiler warnings
- [ ] Security review completed
- [ ] Performance testing done
- [ ] Database backups configured
- [ ] Error logging setup
- [ ] HTTPS configured
- [ ] Authentication implemented
- [ ] Documentation updated
- [ ] Ready for production

---

**Happy Coding!** 🎉

