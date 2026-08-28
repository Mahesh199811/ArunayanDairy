import { useEffect, useState } from "react";
import { getProducts } from "../services/productService";

interface Product {
  id: string;
  name: string;
  description: string;
  price: number;
  unit: string;
  availableQuantity: number;
  availableDate: string;
}

export default function Products() {
  const [products, setProducts] =
    useState<Product[]>([]);

  useEffect(() => {
    loadProducts();
  }, []);

  async function loadProducts() {
    try {
      const data = await getProducts();

      setProducts(data);
    } catch (error) {
      console.error(error);
    }
  }

  return (
    <div>
      <h1>Available Dairy Products</h1>

      {products.map((product) => (
        <div key={product.id}>
          <h2>{product.name}</h2>

          <p>
            {product.description}
          </p>

          <p>
            ₹{product.price} / {product.unit}
          </p>

          <p>
            Available:
            {" "}
            {product.availableQuantity}
          </p>

          <p>
            Date:
            {" "}
            {new Date(
              product.availableDate
            ).toLocaleDateString()}
          </p>
        </div>
      ))}
    </div>
  );
}
