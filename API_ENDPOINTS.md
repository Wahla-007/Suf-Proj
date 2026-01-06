# API Endpoints & Routes Documentation

## Overview
This document outlines all available routes and endpoints in the Mess Management System.

---

## Home Controller Routes

### Dashboard & Landing
```
GET /Home/Index
├─ Description: Home page with portal selection
├─ Access: Public
├─ Returns: Home portal selection page
└─ View: Views/Home/Index.cshtml
```

### Privacy
```
GET /Home/Privacy
├─ Description: Privacy policy page
├─ Access: Public
├─ Returns: Privacy information page
└─ View: Views/Home/Privacy.cshtml
```

### Error Handling
```
GET /Home/Error
├─ Description: Error page display
├─ Access: Public
├─ Returns: Error details
└─ View: Views/Home/Error.cshtml
```

---

## Teacher Controller Routes

### Dashboard
```
GET /Teacher/Dashboard
├─ Description: Teacher portal main dashboard
├─ Access: Teacher
├─ Permissions: View own data only
├─ Returns: Dashboard with:
│   ├─ Attendance statistics
│   ├─ Recent records (top 5)
│   ├─ Current menu rates
│   └─ Monthly bills summary
└─ View: Views/Teacher/Dashboard.cshtml
```

### Attendance History
```
GET /Teacher/AttendanceHistory
├─ Description: View all attendance records
├─ Access: Teacher
├─ Permissions: View own records only
├─ Returns: Table with:
│   ├─ Date
│   ├─ Meals (Breakfast/Lunch/Dinner)
│   ├─ Marked By
│   ├─ Status (Verified/Pending/Rejected)
│   └─ Verification notes
└─ View: Views/Teacher/AttendanceHistory.cshtml
```

### My Bills
```
GET /Teacher/MyBills
├─ Description: View all monthly bills
├─ Access: Teacher
├─ Permissions: View own bills only
├─ Returns: Table with:
│   ├─ Month
│   ├─ Meal counts
│   ├─ Total amount
│   ├─ Payment status
│   └─ Summary totals (Due/Paid/Grand Total)
└─ View: Views/Teacher/MyBills.cshtml
```

### View Menu
```
GET /Teacher/ViewMenu
├─ Description: View weekly menus
├─ Access: Teacher
├─ Permissions: View all menus (public)
├─ Returns: Card view with:
│   ├─ Week start date
│   ├─ Breakfast rate
│   ├─ Lunch rate
│   ├─ Dinner rate
│   └─ Creator details
└─ View: Views/Teacher/ViewMenu.cshtml
```

---

## AspNetUser Controller Routes

### Index (List)
```
GET /AspNetUser/Index
├─ Description: List all users
├─ Access: Admin
├─ Returns: Table with all teachers
└─ Action: Index()
```

### Create (New User)
```
GET /AspNetUser/Create
├─ Description: Show create user form
├─ Access: Admin
├─ Returns: Empty form
└─ View: Views/AspNetUser/Create.cshtml

POST /AspNetUser/Create
├─ Description: Submit new user
├─ Access: Admin
├─ Parameters: AspNetUser model
├─ Returns: Redirect to Index on success
└─ Action: Create(AspNetUser model)
```

### Details
```
GET /AspNetUser/Details/{id}
├─ Description: View user details
├─ Access: Admin
├─ Parameters: id (string) - User ID
├─ Returns: User details view
└─ View: Views/AspNetUser/Details.cshtml
```

### Edit
```
GET /AspNetUser/Edit/{id}
├─ Description: Show edit user form
├─ Access: Admin
├─ Parameters: id (string) - User ID
├─ Returns: Pre-filled form
└─ View: Views/AspNetUser/Edit.cshtml

POST /AspNetUser/Edit/{id}
├─ Description: Submit user updates
├─ Access: Admin
├─ Parameters: id, AspNetUser model
├─ Returns: Redirect to Index on success
└─ Action: Edit(string id, AspNetUser model)
```

### Delete
```
GET /AspNetUser/Delete/{id}
├─ Description: Show delete confirmation
├─ Access: Admin
├─ Parameters: id (string) - User ID
├─ Returns: Confirmation view
└─ View: Views/AspNetUser/Delete.cshtml

POST /AspNetUser/Delete/{id}
├─ Description: Confirm user deletion
├─ Access: Admin
├─ Parameters: id (string)
├─ Returns: Redirect to Index
└─ Action: DeleteConfirmed(string id)
```

---

## WeeklyMenu Controller Routes

### Index (List)
```
GET /WeeklyMenu/Index
├─ Description: List all menus
├─ Access: Admin
├─ Returns: Table with all weekly menus
└─ Action: Index()
```

### Create (New Menu)
```
GET /WeeklyMenu/Create
├─ Description: Show create menu form
├─ Access: Admin
├─ Returns: Empty form
└─ View: Views/WeeklyMenu/Create.cshtml

POST /WeeklyMenu/Create
├─ Description: Submit new menu
├─ Access: Admin
├─ Parameters: WeeklyMenu model
├─ Returns: Redirect to Index on success
└─ Action: Create(WeeklyMenu model)
```

### Details
```
GET /WeeklyMenu/Details/{id}
├─ Description: View menu details
├─ Access: Admin
├─ Parameters: id (int) - Menu ID
├─ Returns: Menu details
└─ View: Views/WeeklyMenu/Details.cshtml
```

### Edit
```
GET /WeeklyMenu/Edit/{id}
├─ Description: Show edit menu form
├─ Access: Admin
├─ Parameters: id (int) - Menu ID
├─ Returns: Pre-filled form
└─ View: Views/WeeklyMenu/Edit.cshtml

POST /WeeklyMenu/Edit/{id}
├─ Description: Submit menu updates
├─ Access: Admin
├─ Parameters: id, WeeklyMenu model
├─ Returns: Redirect to Index
└─ Action: Edit(int id, WeeklyMenu model)
```

### Delete
```
GET /WeeklyMenu/Delete/{id}
├─ Description: Show delete confirmation
├─ Access: Admin
├─ Parameters: id (int) - Menu ID
├─ Returns: Confirmation view
└─ View: Views/WeeklyMenu/Delete.cshtml

POST /WeeklyMenu/Delete/{id}
├─ Description: Confirm menu deletion
├─ Access: Admin
├─ Parameters: id (int)
├─ Returns: Redirect to Index
└─ Action: DeleteConfirmed(int id)
```

---

## TeacherAttendance Controller Routes

### Index (List)
```
GET /TeacherAttendance/Index
├─ Description: List all attendance records
├─ Access: Admin
├─ Returns: Table with all attendance
└─ Action: Index()
```

### Create (New Attendance)
```
GET /TeacherAttendance/Create
├─ Description: Show create attendance form
├─ Access: Admin
├─ Returns: Form with teacher dropdown
└─ View: Views/TeacherAttendance/Create.cshtml

POST /TeacherAttendance/Create
├─ Description: Submit new attendance
├─ Access: Admin
├─ Parameters: TeacherAttendance model
├─ Returns: Redirect to Index on success
└─ Action: Create(TeacherAttendance model)
```

### Details
```
GET /TeacherAttendance/Details/{id}
├─ Description: View attendance details
├─ Access: Admin
├─ Parameters: id (int) - Attendance ID
├─ Returns: Attendance details
└─ View: Views/TeacherAttendance/Details.cshtml
```

### Edit (Verify Attendance)
```
GET /TeacherAttendance/Edit/{id}
├─ Description: Show edit attendance form
├─ Access: Admin
├─ Parameters: id (int) - Attendance ID
├─ Returns: Pre-filled form for verification
└─ View: Views/TeacherAttendance/Edit.cshtml

POST /TeacherAttendance/Edit/{id}
├─ Description: Submit attendance updates & verification
├─ Access: Admin
├─ Parameters: id, TeacherAttendance model with:
│   ├─ IsVerified (bool)
│   ├─ VerificationNote (string)
│   └─ VerifiedAt (DateTime)
├─ Returns: Redirect to Index
└─ Action: Edit(int id, TeacherAttendance model)
```

### Delete
```
GET /TeacherAttendance/Delete/{id}
├─ Description: Show delete confirmation
├─ Access: Admin
├─ Parameters: id (int) - Attendance ID
├─ Returns: Confirmation view
└─ View: Views/TeacherAttendance/Delete.cshtml

POST /TeacherAttendance/Delete/{id}
├─ Description: Confirm attendance deletion
├─ Access: Admin
├─ Parameters: id (int)
├─ Returns: Redirect to Index
└─ Action: DeleteConfirmed(int id)
```

---

## MonthlyBill Controller Routes

### Index (List)
```
GET /MonthlyBill/Index
├─ Description: List all bills
├─ Access: Admin
├─ Returns: Table with all bills
└─ Action: Index()
```

### Create (New Bill)
```
GET /MonthlyBill/Create
├─ Description: Show create bill form
├─ Access: Admin
├─ Returns: Empty form
└─ View: Views/MonthlyBill/Create.cshtml

POST /MonthlyBill/Create
├─ Description: Submit new bill
├─ Access: Admin
├─ Parameters: MonthlyBill model
├─ Returns: Redirect to Index on success
└─ Action: Create(MonthlyBill model)
```

### Details
```
GET /MonthlyBill/Details/{id}
├─ Description: View bill details
├─ Access: Admin/Teacher (own bills)
├─ Parameters: id (int) - Bill ID
├─ Returns: Bill details
└─ View: Views/MonthlyBill/Details.cshtml
```

### Edit
```
GET /MonthlyBill/Edit/{id}
├─ Description: Show edit bill form
├─ Access: Admin
├─ Parameters: id (int) - Bill ID
├─ Returns: Pre-filled form
└─ View: Views/MonthlyBill/Edit.cshtml

POST /MonthlyBill/Edit/{id}
├─ Description: Submit bill updates
├─ Access: Admin
├─ Parameters: id, MonthlyBill model
├─ Returns: Redirect to Index
└─ Action: Edit(int id, MonthlyBill model)
```

### Delete
```
GET /MonthlyBill/Delete/{id}
├─ Description: Show delete confirmation
├─ Access: Admin
├─ Parameters: id (int) - Bill ID
├─ Returns: Confirmation view
└─ View: Views/MonthlyBill/Delete.cshtml

POST /MonthlyBill/Delete/{id}
├─ Description: Confirm bill deletion
├─ Access: Admin
├─ Parameters: id (int)
├─ Returns: Redirect to Index
└─ Action: DeleteConfirmed(int id)
```

---

## URL Pattern Reference

```
/[Controller]/[Action]/[Id]

Examples:
/Home/Index                    → Home page
/Teacher/Dashboard             → Teacher dashboard
/AspNetUser/Index              → Users list
/WeeklyMenu/Create             → Create menu form
/TeacherAttendance/Edit/5      → Edit attendance record 5
/MonthlyBill/Delete/3          → Delete bill 3
```

---

## HTTP Methods

| Method | Purpose | Example |
|--------|---------|---------|
| GET | Retrieve/Display | `/Teacher/Dashboard` |
| POST | Create/Update | Submit form data |
| PUT | Update | Not currently used |
| DELETE | Remove | Not directly exposed |

---

## Status Codes & Responses

```
200 OK
├─ Successful GET request
└─ Returns view or data

201 Created
├─ Successful POST (new record)
└─ Redirects to listing page

400 Bad Request
├─ Invalid form data
└─ Returns form with errors

404 Not Found
├─ Record not found
└─ Returns NotFound view

500 Internal Server Error
├─ Server error
└─ Returns Error view
```

---

## Authentication & Authorization

**JWT Implementation**:
- The system now uses **JWT (JSON Web Token)** for authentication.
- Authentication is stateless; no cookies are used for API requests.
- All protected requests must include the `Authorization: Bearer <token>` header.

**Authentication Flow**:
1. Post credentials to `/api/Auth/login`
2. Receive `accessToken` and `refreshToken`
3. Use `accessToken` in the `Authorization` header for subsequent requests
4. When `accessToken` expires, use `refreshToken` at `/api/Auth/refresh` to get a new one

---

## Auth Controller (API)

### Login
```
POST /api/Auth/login
├─ Description: Authenticate and receive JWT tokens
├─ Access: Public
├─ Parameters (JSON): { "email": "...", "password": "..." }
├─ Returns: { "accessToken": "...", "refreshToken": "...", "expiresAt": "...", "user": { ... } }
└─ Action: Login(LoginViewModel)
```

### Refresh Token
```
POST /api/Auth/refresh
├─ Description: Get a new access token using a refresh token
├─ Access: Public (with valid refresh token)
├─ Parameters (JSON): { "refreshToken": "..." }
├─ Returns: { "accessToken": "...", "expiresAt": "..." }
└─ Action: Refresh(RefreshTokenRequest)
```

### Revoke Token
```
POST /api/Auth/revoke
├─ Description: Revoke a refresh token
├─ Access: Authorized
├─ Parameters (JSON): { "refreshToken": "..." }
├─ Returns: { "message": "Token revoked" }
└─ Action: Revoke(RevokeTokenRequest)
```

### Current User info
```
GET /api/Auth/me
├─ Description: Get current authenticated user details
├─ Access: Authorized
├─ Returns: User info object
└─ Action: GetMe()
```

---

## Security Requirements

- **Header**: `Authorization: Bearer <your_jwt_token>`
- **Token Type**: JWT
- **Hashing**: BCrypt for passwords
- **Expiration**: 
  - Access Token: 60 minutes
  - Refresh Token: 7 days

---

## Form Submissions

### Create/Edit Forms
```
POST to same controller action
├─ ModelState validation
├─ Database update/insert
├─ Redirect to Index on success
└─ Return form on validation error
```

### Model Binding
```csharp
// Automatic binding from form data
[HttpPost]
public IActionResult Create(AspNetUser model)
{
    // model.Id, model.FullName, etc. are populated
    // from form submission
}
```

---

## Query Parameters (Optional)

Currently not implemented, but could be added for:
- Filtering: `/TeacherAttendance/Index?verified=true`
- Sorting: `/MonthlyBill/Index?sortBy=date`
- Pagination: `/AspNetUser/Index?page=2&pageSize=10`

---

## Navigation Flow

```
Home Page
├── Admin Panel
│   ├── Home/Index
│   ├── AspNetUser/* (CRUD)
│   ├── WeeklyMenu/* (CRUD)
│   ├── TeacherAttendance/* (CRUD + Verify)
│   └── MonthlyBill/* (CRUD)
│
└── Teacher Portal
    ├── Teacher/Dashboard
    ├── Teacher/AttendanceHistory
    ├── Teacher/MyBills
    └── Teacher/ViewMenu
```

---

## API Response Examples

### Successful GET (Index)
```html
HTTP/1.1 200 OK
Content-Type: text/html

<!-- Rendered view with table/cards -->
```

### Successful POST (Create)
```html
HTTP/1.1 302 Found
Location: /Controller/Index

<!-- Redirect to listing page -->
```

### Validation Error
```html
HTTP/1.1 200 OK
Content-Type: text/html

<!-- Form re-rendered with error messages -->
```

---

**Last Updated**: December 9, 2025
**Version**: 1.0
