import { useEffect, useState } from "react";

interface HeaderProps {
  userName?: string;
}

export default function Header({ userName }: HeaderProps) {
  const [scrolled, setScrolled] = useState(false);

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
      <a href="#top" className="brand">
        <span className="brand-mark" aria-hidden="true">
          A
        </span>
        <span>
          <strong>Arunayan Dairy</strong>
          <small>Farm fresh, daily</small>
        </span>
      </a>

      <nav className="nav">
        <a href="#products">Products</a>
        <a href="#account">{userName ? "Account" : "Sign in"}</a>
      </nav>

      {userName ? (
        <p className="welcome">Namaste, {userName.split(" ")[0]}</p>
      ) : (
        <a className="header-cta" href="#account">
          Order fresh milk
        </a>
      )}
    </header>
  );
}
