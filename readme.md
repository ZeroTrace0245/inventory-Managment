# Inventory Management System

## Executive Summary
The **Food Shop Inventory Management** system is a modern web-based application built with **Blazor Server** (.NET 10) that enables food shop managers to efficiently track products, suppliers, stock levels, and inventory movements. The system also features AI-powered capabilities through GitHub Models integration for intelligent stock analysis and recommendations.

---

## 1. Technology Stack

### **Backend Framework**
- **Runtime**: .NET 10 (Latest long-term support)
- **Web Framework**: ASP.NET Core with Blazor Server
- **Language**: C# 13 (with modern features like records, nullable reference types, and implicit usings)

### **Frontend**
- **UI Framework**: Blazor Server (Server-side Razor Components)
- **Styling**: Bootstrap 5 + Custom CSS
- **Interactivity**: Interactive Server Components (real-time updates)
- **Scripting**: JavaScript for advanced DOM manipulation (where needed)

### **State Management**
- **In-Memory Store Pattern**: Custom Blazor services managing application state
- **Dependency Injection**: ASP.NET Core DI container
- **Scoped Services**: Per-session state isolation

### **AI Integration**
- **AI Provider**: GitHub Models (gpt-4o-mini by default)
- **API**: GitHub Copilot Models REST API
- **Endpoint**: `https://models.github.ai/inference/chat/completions`
- **Authentication**: GitHub Personal Access Token

### **Data Storage**
- **Current Implementation**: In-memory collections with seeded demo data
- **Architecture**: Ready for database integration (compatible with Entity Framework Core)

### **Security Features**
- **Authentication**: Session-based login system
- **Authorization**: Protected pages requiring authentication
- **HTTPS**: Enforced in production
- **Antiforgery Protection**: Built-in CSRF protection
- **Nullable Reference Types**: Enabled for type safety

---

## 2. Architecture & How Components Work Together

### **2.1 Application Flow Diagram**
```
User Browser (Client)
	↓
Blazor Server (Interactive Components)
	↓
State Management Services
├── InventoryStore (Product, Supplier, Category, Stock data)
├── AppSessionStore (User authentication & session)
└── AppPreferencesStore (User preferences)
	↓
Razor Components / Pages
├── Dashboard (Home.razor)
├── Products Management (Products.razor)
├── Suppliers Management (Suppliers.razor)
├── Stock Adjustments (Stock.razor)
└── AI Labs (Labs.razor)
```

### **2.2 Core Services**

#### **InventoryStore** (`Inventory/InventoryStore.cs`)
**Purpose**: Centralized inventory data management

**Key Features**:
- Manages Categories, Suppliers, Products, and Stock Movements
- Provides dashboard metrics (product count, low stock alerts, stock values)
- Implements change notifications for reactive UI updates
- CRUD operations: Add, Update, Remove products/suppliers/categories
- Stock management: Adjust quantities with audit trail

**Key Methods**:
```csharp
GetDashboard()              // Returns summary metrics
GetProductOverviews()       // Detailed product information with joins
AdjustStock()              // Record stock changes with reason
GetRecentMovements()       // Audit trail of inventory changes
AddProduct/Category/Supplier() // Create domain entities
```

#### **AppSessionStore** (`Inventory/AppSessionStore.cs`)
**Purpose**: User session and authentication management

**Key Features**:
- Tracks login state
- Manages user profile (display name)
- Provides authentication checks

#### **AppPreferencesStore** (`Inventory/AppPreferencesStore.cs`)
**Purpose**: User preferences and settings

**Key Features**:
- GitHub Models API token storage
- Theme preferences (Light/Dark mode)
- Token validation
- Persistent storage via `AppPreferencesStorage`

---

## 3. Page Architecture

### **Dashboard (Home.razor)**
**Route**: `/`
- **Purpose**: Overview of inventory status
- **Components**:
  - Metric cards (Product count, Categories, Suppliers, Low Stock alerts)
  - Low stock alert table with real-time status
  - Recent stock movements
  - Authentication state display
- **Data Source**: `InventoryStore.GetDashboard()`

### **Products Management (Products.razor)**
**Route**: `/products`
- **Purpose**: Full CRUD operations for products
- **Features**:
  - Add new products
  - Edit existing products
  - View product details with pricing and inventory
  - Filter by category/supplier
  - Delete products (soft delete - marked inactive)
- **Data Validation**: Category and supplier existence checks

### **Suppliers Management (Suppliers.razor)**
**Route**: `/suppliers`
- **Purpose**: Manage supplier information
- **Features**:
  - Add/Edit suppliers
  - Store contact information (name, phone, email)
  - Link to products

### **Stock Management (Stock.razor)**
**Route**: `/stock`
- **Purpose**: Adjust inventory quantities
- **Features**:
  - Record stock movements
  - Prevent negative stock
  - Audit trail with reason/notes
  - Validates reorder levels

### **AI Labs (Labs.razor)** ⭐ NEW
**Route**: `/labs`
- **Purpose**: Experimental AI features
- **Features**:
  - AI Assistant powered by GitHub Models
  - Context-aware prompts using live inventory data
  - Model selection (configurable - defaults to gpt-4o-mini)
  - Conversation history
  - Token requirement notification
- **AI Capabilities**:
  - Stock analysis and recommendations
  - Low stock predictions
  - Supplier recommendations
  - Shop management advice

---

## 4. State Management Pattern

### **How Real-Time Updates Work**

1. **User Action** (e.g., add product)
   ```
   Component → InventoryStore.AddProduct() 
   ```

2. **Store Notifies Change**
   ```csharp
   private void NotifyChanged() => Changed?.Invoke();
   ```

3. **Blazor Detects Change**
   - Components subscribed to `InventoryStore.Changed` event
   - `StateHasChanged()` called automatically

4. **UI Re-renders**
   - Components re-execute their render logic
   - Updated data displays to user

### **Service Registration** (`Program.cs`)
```csharp
builder.Services.AddInventory();              // Registers all inventory services
builder.Services.AddScoped<AppPreferencesStorage>(); // Per-session preferences
```

---

## 5. Data Model

### **Domain Entities**

#### **Product**
```csharp
public record Product(
	int Id,
	string Name,
	string Sku,
	int CategoryId,
	int SupplierId,
	string Unit,
	decimal CostPrice,
	decimal SalePrice,
	int QuantityOnHand,
	int ReorderLevel,
	DateOnly? ExpiryDate,
	bool IsActive
)
```

#### **Category**
```csharp
public record Category(
	int Id,
	string Name,
	string Description
)
```

#### **Supplier**
```csharp
public record Supplier(
	int Id,
	string Name,
	string ContactName,
	string Phone,
	string Email
)
```

#### **StockMovement** (Audit Trail)
```csharp
public record StockMovement(
	int Id,
	int ProductId,
	int QuantityChange,
	string Reason,
	string Notes,
	DateTimeOffset OccurredAt
)
```

---

## 6. AI Integration Architecture

### **GitHub Models Integration**

**Authentication Flow**:
1. User enters GitHub Personal Access Token in Settings
2. Token stored in `AppPreferencesStore`
3. Token included in API requests header

**Request Flow** (Labs.razor):
```
User Input
	↓
Build Context
├── System Prompt (AI role definition)
├── Inventory Summary (20+ data points)
├── Conversation History (last 10 messages)
└── User Question
	↓
POST to https://models.github.ai/inference/chat/completions
	↓
Process Response
	↓
Display in Chat Interface
```

**System Prompt Example**:
```
"You are a helpful inventory assistant for a food shop. 
The current user is [User Name]. 
Use the provided inventory summary to answer with practical 
shop-management advice. Inventory summary: 
[20 products, 4 categories, 4 suppliers, 5 low stock, 
1 out of stock...]"
```

**API Request Structure**:
```json
{
  "model": "gpt-4o-mini",
  "messages": [
	{"role": "system", "content": "System prompt..."},
	{"role": "user", "content": "What items are low in stock?"},
	{"role": "assistant", "content": "Based on your inventory..."}
  ],
  "temperature": 0.2,
  "max_tokens": 800
}
```

---

## 7. Security & Best Practices

### **Authentication & Authorization**
- ✅ Session-based authentication
- ✅ Protected routes (require `IsAuthenticated`)
- ✅ Automatic redirect to login for unauthorized access
- ✅ Secure token storage for GitHub API

### **Data Validation**
- ✅ Input trimming and validation
- ✅ Category/Supplier existence checks
- ✅ Negative stock prevention
- ✅ Nullable reference types for null-safety

### **Code Quality**
- ✅ C# Records for immutability
- ✅ LINQ for data queries
- ✅ Event-driven state updates
- ✅ Proper error handling with try-catch in async operations

### **Frontend Security**
- ✅ HTTPS enforced
- ✅ Antiforgery tokens
- ✅ HTTP status code page rewriting (404 handling)
- ✅ Exception handling middleware

---

## 8. Blazor Server Advantages Used

1. **Real-Time Reactivity**: Changes instantly reflect across all connected users
2. **Server-Side Processing**: Heavy lifting done on server, thin client
3. **Type Safety**: Full C# compilation and intellisense
4. **Stateful Components**: Session-maintained data without page reloads
5. **Seamless Integration**: C# on both frontend and backend
6. **Event System**: Two-way data binding and event handling

---

## 9. Performance Characteristics

### **In-Memory Storage Benefits**
- ⚡ Instant data access (O(1) lookups)
- ⚡ No database latency
- ⚡ Real-time change notifications
- ⚡ Suitable for MVP and demo purposes

### **Optimization Techniques**
- ReadOnlyCollection for immutable exposure
- LINQ query lazy evaluation
- Efficient change notification system
- Scoped service lifetime for session isolation

### **Scalability Path**
The architecture is designed to transition to persistent storage:
- Replace `InventoryStore` in-memory lists with EF Core DbSet
- Keep the same public API
- Add database migrations
- Minimal code changes required

---

## 10. Technology Integration Map

```
┌─────────────────────────────────────────────────────────────┐
│                    User Browser                             │
│              (HTML5 + JavaScript)                          │
└──────────────────────────┬──────────────────────────────────┘
						   │ WebSocket (SignalR)
						   ↓
┌─────────────────────────────────────────────────────────────┐
│              ASP.NET Core 10 Application                    │
│  ┌────────────────────────────────────────────────────────┐ │
│  │  Blazor Server (Interactive Components)               │ │
│  │  ┌──────────────────────────────────────────────────┐ │ │
│  │  │ Pages (Home, Products, Suppliers, Stock, Labs)  │ │ │
│  │  │ Components (Forms, Tables, Chat Interface)       │ │ │
│  │  └──────────────────────────────────────────────────┘ │ │
│  └────────────────────────────────────────────────────────┘ │
│  ┌────────────────────────────────────────────────────────┐ │
│  │  State Management Services (DI Container)             │ │
│  │  ├── InventoryStore                                   │ │
│  │  ├── AppSessionStore                                  │ │
│  │  └── AppPreferencesStore                              │ │
│  └────────────────────────────────────────────────────────┘ │
│  ┌────────────────────────────────────────────────────────┐ │
│  │  Data Layer (In-Memory Collections)                   │ │
│  │  ├── Products List                                    │ │
│  │  ├── Categories List                                  │ │
│  │  ├── Suppliers List                                   │ │
│  │  └── Stock Movements List                             │ │
│  └────────────────────────────────────────────────────────┘ │
└──────────────────────┬───────────────────┬──────────────────┘
					   │                   │
					[Future EF Core        │
					 Database]             │
										   ↓
						  ┌────────────────────────────────┐
						  │  GitHub Models API             │
						  │  gpt-4o-mini LLM               │
						  └────────────────────────────────┘
```

---

## 11. User Workflows

### **Workflow 1: Login & Dashboard**
1. User lands on `/` (Home page)
2. Authentication check: If not authenticated, shows "Sign in" button
3. Click "Sign in" → Navigate to `/login`
4. Enter credentials → `AppSessionStore` validates
5. Redirect to Dashboard with personalized greeting
6. Displays real-time inventory metrics

### **Workflow 2: Manage Inventory**
1. From Dashboard: Click "Manage products" → `/products`
2. View all products in table
3. Click "Add new" → Form opens
4. Fill product details (name, SKU, category, supplier, pricing, quantity)
5. Submit → `InventoryStore.AddProduct()` called
6. `Changed` event fires → All subscribed components re-render
7. Dashboard updates automatically (no page reload)

### **Workflow 3: AI Stock Analysis**
1. Navigate to `/labs` (AI Labs page)
2. See token requirement notice
3. Go to Settings (gear icon)
4. Enter GitHub API token
5. Return to Labs
6. Type question: "What items need reordering?"
7. Assistant analyzes live inventory data
8. Returns AI-powered recommendations
9. Multi-turn conversation maintained in session

---

## 12. Key Features & Capabilities

### **Inventory Management**
- ✅ Product CRUD with 20+ data points
- ✅ Category organization
- ✅ Supplier tracking
- ✅ Expiry date management
- ✅ Stock level alerts
- ✅ Reorder level configuration

### **Stock Tracking**
- ✅ Real-time quantity adjustments
- ✅ Complete audit trail (who, what, when, why)
- ✅ Reason documentation
- ✅ Movement history with timestamps
- ✅ Bulk operations support

### **Analytics & Reporting**
- ✅ Dashboard metrics (at-a-glance overview)
- ✅ Low stock detection
- ✅ Out of stock alerts
- ✅ Stock value calculations (cost & retail)
- ✅ Movement history reports

### **AI-Powered Insights** ⭐
- ✅ Context-aware recommendations
- ✅ Multi-turn conversations
- ✅ Live inventory integration
- ✅ Custom model selection
- ✅ Reason/notes documentation

---

## 13. Deployment & Runtime

### **System Requirements**
- **.NET Runtime**: 10.0+ (or .NET 10 SDK for development)
- **Browser**: Modern browser with WebSocket support (Chrome, Edge, Firefox, Safari)
- **Server**: Single machine sufficient for MVP (scales with SignalR backplane)

### **Running the Application**
```bash
# Development
dotnet run

# Production build
dotnet publish -c Release

# Docker (can be containerized)
docker build .
docker run -p 5000:5000 inventory-management
```

### **Configuration**
- `appsettings.json`: Logging and host configuration
- `Program.cs`: Service registration and middleware pipeline
- Environment: Development includes detailed error pages

---

## 14. Future Enhancement Opportunities

### **Database Integration**
```csharp
// Replace in-memory with EF Core
builder.Services.AddDbContext<InventoryDbContext>(options =>
	options.UseSqlServer(connectionString));
```

### **Advanced Features**
- Barcode scanning for faster stock updates
- Mobile app (Blazor Hybrid)
- Predictive analytics (ML.NET)
- Integration with POS systems
- Multi-location inventory sync
- Real-time reporting dashboard

### **Performance Scaling**
- SignalR backplane for multi-server deployments
- Redis caching layer
- Database query optimization
- API rate limiting

---

## Conclusion

The **Inventory Management System** demonstrates a modern, full-stack web application using cutting-edge .NET technologies. It combines:

- **Blazor Server** for real-time, interactive UI
- **ASP.NET Core 10** for robust backend
- **GitHub Models AI** for intelligent assistance
- **Clean architecture** with separation of concerns
- **Session-based state management** for scalability

The system is production-ready for food shop inventory operations and includes experimental AI capabilities that provide intelligent recommendations based on live business data.

---
