import { useState } from "react";
import Header from "./components/Header";
import Login from "./pages/Login";
import Products from "./pages/Products";

function readStoredName() {
  try {
    const raw = localStorage.getItem("user");
    if (!raw) {
      return "";
    }

    return JSON.parse(raw).fullName as string;
  } catch {
    return "";
  }
}

function App() {
  const [userName, setUserName] = useState(readStoredName);

  return (
    <div id="top" className="page">
      <Header userName={userName} />

      <section className="hero">
        <img src="/images/dairy-hero.png" alt="" className="hero-image" />
        <div className="hero-veil" />
        <div className="hero-copy">
          <p className="eyebrow light">Since the first light</p>
          <h1>Pure milk. Honest dairy. Delivered to your home.</h1>
          <p>
            Arunayan Dairy brings farm-fresh cow milk, curd, and paneer from
            our meadows to your table — still cool, still clean.
          </p>
          <div className="hero-actions">
            <a className="btn-primary" href="#products">
              See today's products
            </a>
            <a className="btn-ghost" href="#account">
              Sign in to order
            </a>
          </div>
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
      <Login onSignedIn={setUserName} />

      <footer className="site-footer">
        <p>Arunayan Dairy · Farm to family</p>
        <p>Fresh dairy for every household.</p>
      </footer>
    </div>
  );
}

export default App;
