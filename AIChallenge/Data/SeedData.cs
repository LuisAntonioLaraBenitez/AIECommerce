using AIChallenge.Models;

namespace AIChallenge.Data;

public static class SeedData
{
    public static AppData Create()
    {
        return new AppData
        {
            Products = CreateProducts()
        };
    }

    public static bool EnsureProducts(AppData data)
    {
        bool added = false;
        foreach (Product product in CreateProducts())
        {
            if (data.Products.Any(existing => string.Equals(existing.Sku, product.Sku, StringComparison.OrdinalIgnoreCase)))
            {
                continue;
            }

            data.Products.Add(product);
            added = true;
        }

        return added;
    }

    private static List<Product> CreateProducts()
    {
        return
        [
            new Product(
                "SKU-LAP-001",
                "Laptop ultraligera",
                4299.00m,
                ["14 pulgadas", "8 GB RAM", "256 GB SSD"]),
            new Product(
                "SKU-MOU-002",
                "Mouse inalámbrico",
                349.00m,
                ["Bluetooth", "Batería recargable", "Garantía 1 año"]),
            new Product(
                "SKU-KEY-003",
                "Teclado mecánico",
                899.00m,
                ["Switch azul", "Distribución español", "Retroiluminado"]),
            new Product(
                "SKU-HED-004",
                "Audífonos con micrófono",
                699.00m,
                ["Cancelación pasiva", "Cable USB-C", "Controles integrados"])
        ];
    }
}
