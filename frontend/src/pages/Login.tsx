import { FormEvent, useState } from "react";
import { loginUser, registerUser } from "../services/userService";

interface LoginProps {
  onSignedIn: (fullName: string) => void;
}

export default function Login({ onSignedIn }: LoginProps) {
  const [mode, setMode] = useState<"login" | "register">("login");
  const [fullName, setFullName] = useState("");
  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");
  const [message, setMessage] = useState("");
  const [error, setError] = useState("");
  const [loading, setLoading] = useState(false);

  async function handleSubmit(event: FormEvent) {
    event.preventDefault();
    setError("");
    setMessage("");
    setLoading(true);

    try {
      if (mode === "register") {
        await registerUser({ fullName, email, password });
        const result = await loginUser({ email, password });
        localStorage.setItem("token", result.token);
        localStorage.setItem("user", JSON.stringify(result.user));
        onSignedIn(result.user?.fullName ?? "");
        setMessage("Welcome to Arunayan Dairy. You are signed in.");
      } else {
        const result = await loginUser({ email, password });
        localStorage.setItem("token", result.token);
        localStorage.setItem("user", JSON.stringify(result.user));
        onSignedIn(result.user?.fullName ?? "");
        setMessage("Signed in. Browse today's dairy.");
      }
    } catch (err) {
      console.error(err);
      setError(
        mode === "register"
          ? "Could not create your account. Try another email."
          : "Login failed. Check your email and password."
      );
    } finally {
      setLoading(false);
    }
  }

  return (
    <section id="account" className="account-section">
      <div className="account-photo">
        <img src="/images/dairy-hero.jpg" alt="Cows grazing at Arunayan farm" />
        <div className="account-photo-copy">
          <p>From our farm</p>
          <h2>Milk collected at dawn, at your door the same day.</h2>
        </div>
      </div>

      <div className="account-card">
        <p className="eyebrow">Your account</p>
        <h2>{mode === "login" ? "Welcome back" : "Join the dairy family"}</h2>
        <p className="lede">
          Sign in to schedule fresh milk, curd, and paneer for your household.
        </p>

        <div className="tabs" role="tablist">
          <button
            type="button"
            className={mode === "login" ? "is-active" : ""}
            onClick={() => setMode("login")}
          >
            Sign in
          </button>
          <button
            type="button"
            className={mode === "register" ? "is-active" : ""}
            onClick={() => setMode("register")}
          >
            Create account
          </button>
        </div>

        <form onSubmit={handleSubmit}>
          {mode === "register" && (
            <label>
              Full name
              <input
                type="text"
                value={fullName}
                onChange={(e) => setFullName(e.target.value)}
                placeholder="Mahesh Gadhave"
                required
              />
            </label>
          )}

          <label>
            Email
            <input
              type="email"
              value={email}
              onChange={(e) => setEmail(e.target.value)}
              placeholder="you@example.com"
              required
            />
          </label>

          <label>
            Password
            <input
              type="password"
              value={password}
              onChange={(e) => setPassword(e.target.value)}
              placeholder="••••••••"
              required
            />
          </label>

          {error && <p className="form-error">{error}</p>}
          {message && <p className="form-success">{message}</p>}

          <button className="btn-primary" type="submit" disabled={loading}>
            {loading
              ? "Please wait…"
              : mode === "login"
                ? "Sign in"
                : "Create account"}
          </button>
        </form>
      </div>
    </section>
  );
}
