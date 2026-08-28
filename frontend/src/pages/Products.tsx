import { useEffect, useState } from "react";
import { getProducts } from "../services/productService";
import { useCart } from "../context/CartContext";

interface Product {
  id: string;
  name: string;
  description: string;
  price: number;
  unit: string;
  availableQuantity: number;
  availableDate: string;
}

function productImage(name: string) {
  const value = name.toLowerCase();

  if (value.includes("curd") || value.includes("dahi") || value.includes("yogurt")) {
    return "/images/product-curd.png";
  }

  if (value.includes("paneer") || value.includes("cheese") || value.includes("ghee")) {
    return "/images/product-paneer.png";
  }

  return "/images/product-milk.png";
}

export default function Products() {
  const [products, setProducts] = useState<Product[]>([]);
  const [loading, setLoading] = useState(true);
  const [failed, setFailed] = useState(false);
  const [notice, setNotice] = useState("");
  const { addToCart } = useCart();

  useEffect(() => {
    loadProducts();
  }, []);

  async function loadProducts() {
    try {
      const data = await getProducts();
      setProducts(data);
    } catch (error) {
      console.error(error);
      setFailed(true);
    } finally {
      setLoading(false);
    }
  }

  function handleAddToCart(product: Product) {
    addToCart({
      productId: product.id,
      name: product.name,
      price: product.price,
      unit: product.unit,
      quantity: 1,
      availableQuantity: product.availableQuantity,
      availableDate: product.availableDate,
    });

    setNotice(`${product.name} added to cart`);
    window.setTimeout(() => setNotice(""), 2200);
  }

  return (
    <section id="products" className="products-section">
      <div className="section-heading">
        <p className="eyebrow">Today's dairy</p>
        <h2>Fresh from the farm</h2>
        <p className="lede">
          Harvested this morning. Order by evening for tomorrow's delivery.
        </p>
      </div>

      {notice && <p className="toast">{notice}</p>}

      {loading && <p className="status-copy">Bringing in today's batch…</p>}

      {failed && (
        <p className="status-copy">
          We could not reach the dairy counter. Make sure Product Service is running.
        </p>
      )}

      {!loading && !failed && products.length === 0 && (
        <p className="status-copy">
          No products listed yet. Add some from Product Service, then refresh.
        </p>
      )}

      <div className="product-grid">
        {products.map((product, index) => (
          <article
            key={product.id}
            className="product-card"
            style={{ animationDelay: `${index * 80}ms` }}
          >
            <div className="product-photo">
              <img src={productImage(product.name)} alt={product.name} />
              <span className="badge">Fresh</span>
            </div>

            <div className="product-body">
              <h3>{product.name}</h3>
              <p>{product.description}</p>

              <p className="price">
                ₹{product.price}
                <small> / {product.unit}</small>
              </p>

              <div className="meta">
                <span>{product.availableQuantity} {product.unit} left</span>
                <span>
                  {new Date(product.availableDate).toLocaleDateString("en-IN", {
                    day: "numeric",
                    month: "short",
                  })}
                </span>
              </div>

              <button
                type="button"
                className="btn-primary"
                onClick={() => handleAddToCart(product)}
              >
                Add to Cart
              </button>
            </div>
          </article>
        ))}
      </div>
    </section>
  );
}
