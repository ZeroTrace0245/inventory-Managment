using System.Collections.ObjectModel;

namespace inventory_Managment.Inventory;

public sealed class InventoryStore
{
    private readonly List<Category> _categories = new();
    private readonly List<Supplier> _suppliers = new();
    private readonly List<Product> _products = new();
    private readonly List<StockMovement> _movements = new();
    private int _nextCategoryId = 1;
    private int _nextSupplierId = 1;
    private int _nextProductId = 1;
    private int _nextMovementId = 1;

    public InventoryStore()
    {
        Seed();
    }

    public event Action? Changed;

    public IReadOnlyList<Category> Categories => new ReadOnlyCollection<Category>(_categories);
    public IReadOnlyList<Supplier> Suppliers => new ReadOnlyCollection<Supplier>(_suppliers);
    public IReadOnlyList<Product> Products => new ReadOnlyCollection<Product>(_products);
    public IReadOnlyList<StockMovement> StockMovements => new ReadOnlyCollection<StockMovement>(_movements);

    public InventoryDashboard GetDashboard() => new(
        ProductCount: _products.Count,
        CategoryCount: _categories.Count,
        SupplierCount: _suppliers.Count,
        LowStockCount: _products.Count(p => p.IsActive && p.QuantityOnHand > 0 && p.QuantityOnHand <= p.ReorderLevel),
        OutOfStockCount: _products.Count(p => p.IsActive && p.QuantityOnHand <= 0),
        StockCostValue: _products.Where(p => p.IsActive).Sum(p => p.CostPrice * p.QuantityOnHand),
        StockRetailValue: _products.Where(p => p.IsActive).Sum(p => p.SalePrice * p.QuantityOnHand)
    );

    public IEnumerable<ProductOverview> GetProductOverviews() =>
        from product in _products.Where(p => p.IsActive).OrderBy(p => p.Name)
        join category in _categories on product.CategoryId equals category.Id
        join supplier in _suppliers on product.SupplierId equals supplier.Id
        select new ProductOverview(
            product.Id,
            product.Name,
            product.Sku,
            category.Name,
            supplier.Name,
            product.Unit,
            product.CostPrice,
            product.SalePrice,
            product.QuantityOnHand,
            product.ReorderLevel,
            product.ExpiryDate,
            product.QuantityOnHand > 0 && product.QuantityOnHand <= product.ReorderLevel,
            product.QuantityOnHand <= 0);

    public Category AddCategory(string name, string description)
    {
        var category = new Category(_nextCategoryId++, name.Trim(), description.Trim());
        _categories.Add(category);
        NotifyChanged();
        return category;
    }

    public Supplier AddSupplier(string name, string contactName, string phone, string email)
    {
        var supplier = new Supplier(_nextSupplierId++, name.Trim(), contactName.Trim(), phone.Trim(), email.Trim());
        _suppliers.Add(supplier);
        NotifyChanged();
        return supplier;
    }

    public Product AddProduct(ProductDraft draft)
    {
        if (!_categories.Any(c => c.Id == draft.CategoryId))
        {
            throw new InvalidOperationException("Category not found.");
        }

        if (!_suppliers.Any(s => s.Id == draft.SupplierId))
        {
            throw new InvalidOperationException("Supplier not found.");
        }

        var product = new Product(
            _nextProductId++,
            draft.Name.Trim(),
            draft.Sku.Trim(),
            draft.CategoryId,
            draft.SupplierId,
            draft.Unit.Trim(),
            draft.CostPrice,
            draft.SalePrice,
            draft.QuantityOnHand,
            draft.ReorderLevel,
            draft.ExpiryDate,
            true);

        _products.Add(product);
        if (draft.QuantityOnHand != 0)
        {
            _movements.Add(new StockMovement(_nextMovementId++, product.Id, draft.QuantityOnHand, "Opening stock", "Initial stock level", DateTimeOffset.Now));
        }

        NotifyChanged();
        return product;
    }

    public void AdjustStock(int productId, int quantityChange, string reason, string notes)
    {
        var index = _products.FindIndex(p => p.Id == productId);
        if (index < 0)
        {
            throw new InvalidOperationException("Product not found.");
        }

        var product = _products[index];
        var updatedQuantity = product.QuantityOnHand + quantityChange;
        if (updatedQuantity < 0)
        {
            throw new InvalidOperationException("Stock cannot go below zero.");
        }

        _products[index] = product with { QuantityOnHand = updatedQuantity };
        _movements.Insert(0, new StockMovement(_nextMovementId++, productId, quantityChange, reason.Trim(), notes.Trim(), DateTimeOffset.Now));
        NotifyChanged();
    }

    public void UpdateProduct(Product updated)
    {
        var index = _products.FindIndex(p => p.Id == updated.Id);
        if (index < 0)
        {
            throw new InvalidOperationException("Product not found.");
        }

        _products[index] = updated;
        NotifyChanged();
    }

    public void RemoveProduct(int productId)
    {
        var index = _products.FindIndex(p => p.Id == productId);
        if (index < 0)
        {
            return;
        }

        _products[index] = _products[index] with { IsActive = false };
        NotifyChanged();
    }

    public IEnumerable<StockMovement> GetRecentMovements(int count = 10) => _movements.Take(count);

    public IEnumerable<StockMovementDetail> GetRecentMovementDetails(int count = 10)
    {
        return from movement in _movements.Take(count)
               join product in _products on movement.ProductId equals product.Id
               select new StockMovementDetail(
                   movement.Id,
                   product.Name,
                   movement.QuantityChange,
                   movement.Reason,
                   movement.Notes,
                   movement.OccurredAt);
    }

    private void NotifyChanged() => Changed?.Invoke();

    private void Seed()
    {
        var categories = new[]
        {
            AddCategory("Fresh Produce", "Fruits and vegetables"),
            AddCategory("Dairy", "Milk, cheese, yogurt, and cream"),
            AddCategory("Bakery", "Bread, buns, and baked goods"),
            AddCategory("Pantry", "Dry goods, canned items, and condiments")
        };

        var suppliers = new[]
        {
            AddSupplier("Green Farm Co.", "Amina Yusuf", "+1 555 0101", "orders@greenfarm.example"),
            AddSupplier("Daily Dairy Ltd.", "Mark Thomas", "+1 555 0102", "sales@dailydairy.example"),
            AddSupplier("Fresh Bake House", "Lina Chen", "+1 555 0103", "hello@freshbake.example"),
            AddSupplier("Pantry Partners", "Omar Ali", "+1 555 0104", "support@pantrypartners.example")
        };

        AddProduct(new ProductDraft("Bananas", "PROD-1001", categories[0].Id, suppliers[0].Id, "kg", 0.85m, 1.49m, 42, 20, DateOnly.FromDateTime(DateTime.Today.AddDays(7))));
        AddProduct(new ProductDraft("Whole Milk", "PROD-2001", categories[1].Id, suppliers[1].Id, "litre", 1.10m, 1.99m, 16, 24, DateOnly.FromDateTime(DateTime.Today.AddDays(5))));
        AddProduct(new ProductDraft("Sandwich Bread", "PROD-3001", categories[2].Id, suppliers[2].Id, "loaf", 0.95m, 1.75m, 8, 12, DateOnly.FromDateTime(DateTime.Today.AddDays(4))));
        AddProduct(new ProductDraft("Rice 5kg", "PROD-4001", categories[3].Id, suppliers[3].Id, "bag", 7.50m, 10.99m, 22, 10, null));
        AddProduct(new ProductDraft("Tomatoes", "PROD-1002", categories[0].Id, suppliers[0].Id, "kg", 1.30m, 2.20m, 0, 18, DateOnly.FromDateTime(DateTime.Today.AddDays(3))));
    }
}

public sealed record ProductDraft(
    string Name,
    string Sku,
    int CategoryId,
    int SupplierId,
    string Unit,
    decimal CostPrice,
    decimal SalePrice,
    int QuantityOnHand,
    int ReorderLevel,
    DateOnly? ExpiryDate);
