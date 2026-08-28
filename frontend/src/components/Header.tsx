import { useEffect, useState } from "react";
import { useCart } from "../context/CartContext";

export type AppPage = "login" | "products" | "cart" | "orders";

interface HeaderProps {
  userName?: string;
  page: AppPage;
  onNavigate: (page: AppPage) => void;
}

export default function Header({
  userName,
  page,
  onNavigate,
}: HeaderProps) {
  const [scrolled, setScrolled] = useState(false);
  const { items } = useCart();
  const cartCount = items.reduce((sum, item) => sum + item.quantity, 0);

  useEffect(() => {
    function onScroll() {
      setScrolled(window.scrollY > 12);
    }

    onScroll();
    window.addEventListener("scroll", onScroll);
    return () => window.removeEventListener("scroll", onScroll);
  }, []);

  return (
    <header className={`site-header ${scrolled ? "is-scrolled" : ""}`}>
      <button type="button" className="brand" onClick={() => onNavigate("products")}>
        <span className="brand-mark" aria-hidden="true">
          A
        </span>
        <span>
          <strong>Arunayan Dairy</strong>
          <small>Farm fresh, daily</small>
        </span>
      </button>

      <nav className="nav">
        <button
          type="button"
          className={page === "products" ? "is-active" : ""}
          onClick={() => onNavigate("products")}
        >
          Products
        </button>
        <button
          type="button"
          className={page === "cart" ? "is-active" : ""}
          onClick={() => onNavigate("cart")}
        >
          Cart{cartCount > 0 ? ` (${cartCount})` : ""}
        </button>
        <button
          type="button"
          className={page === "orders" ? "is-active" : ""}
          onClick={() => onNavigate("orders")}
        >
          My Orders
        </button>
        <button
          type="button"
          className={page === "login" ? "is-active" : ""}
          onClick={() => onNavigate("login")}
        >
          {userName ? "Account" : "Sign in"}
        </button>
      </nav>

      {userName ? (
        <p className="welcome">Namaste, {userName.split(" ")[0] || "there"}</p>
      ) : (
        <button
          type="button"
          className="header-cta"
          onClick={() => onNavigate("login")}
        >
          Order fresh milk
        </button>
      )}
    </header>
  );
}
