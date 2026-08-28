import { useState } from "react";
import Header, { type AppPage } from "./components/Header";
import Login from "./pages/Login";
import Products from "./pages/Products";
import Cart from "./pages/Cart";
import Orders from "./pages/Orders";
import { readStoredName, signOut } from "./lib/userStorage";
import { useCart } from "./context/CartContext";

function App() {
  const [page, setPage] = useState<AppPage>(
    readStoredName() ? "products" : "login"
  );
  const [userName, setUserName] = useState(readStoredName);
  const { clearCart } = useCart();

  function handleSignOut() {
    signOut();
    clearCart();
    setUserName("");
    setPage("login");
  }

  return (
    <div id="top" className="page">
      <Header
        userName={userName}
        page={page}
        onNavigate={setPage}
        onSignOut={handleSignOut}
      />

      {page === "products" && (
        <>
          <section className="hero">
            <img src="/images/dairy-hero.png" alt="" className="hero-image" />
            <div className="hero-veil" />
            <div className="hero-copy">
              <p className="eyebrow light">Since the first light</p>
              <h1>Pure milk. Honest dairy. Delivered to your home.</h1>
              <p>
                Add today's batch to your cart, pick a delivery date, and we
                will bring it still cool from the farm.
              </p>
            </div>
          </section>

          <section className="trust">
            <article>
              <strong>Dawn collection</strong>
              <p>Milk drawn before sunrise, bottled the same morning.</p>
            </article>
            <article>
              <strong>Local farms</strong>
              <p>Happy cows, green pasture, no long-haul cold chain.</p>
            </article>
            <article>
              <strong>Scheduled delivery</strong>
              <p>Choose the day you want it on your doorstep.</p>
            </article>
          </section>

          <Products />
        </>
      )}

      {page === "login" && (
        <Login
          onSignedIn={(name) => {
            setUserName(name);
            setPage("products");
          }}
        />
      )}

      {page === "cart" && (
        <Cart onOrderPlaced={() => setPage("orders")} />
      )}

      {page === "orders" && <Orders />}

      <footer className="site-footer">
        <p>Arunayan Dairy · Farm to family</p>
        <p>Fresh dairy for every household.</p>
      </footer>
    </div>
  );
}

export default App;
