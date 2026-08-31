import { useState, type FormEvent } from "react";
import { loginUser, registerUser } from "../services/userService";

interface LoginProps {
  onSignedIn: (fullName: string) => void;
}

export default function Login({ onSignedIn }: LoginProps) {
  const [mode, setMode] = useState<"login" | "register">("login");
  const [fullName, setFullName] = useState("");
  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");
  const [showPassword, setShowPassword] = useState(false);
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
            <div className="password-field">
              <input
                type={showPassword ? "text" : "password"}
                value={password}
                onChange={(e) => setPassword(e.target.value)}
                placeholder="••••••••"
                required
              />
              <button
                type="button"
                className="password-toggle"
                onClick={() => setShowPassword((open) => !open)}
                aria-label={showPassword ? "Hide password" : "Show password"}
              >
                {showPassword ? (
                  <svg viewBox="0 0 24 24" aria-hidden="true">
                    <path
                      fill="currentColor"
                      d="M12 5c-7 0-10 7-10 7s3 7 10 7 10-7 10-7-3-7-10-7zm0 12a5 5 0 1 1 0-10 5 5 0 0 1 0 10zm0-2.5a2.5 2.5 0 1 0 0-5 2.5 2.5 0 0 0 0 5z"
                    />
                  </svg>
                ) : (
                  <svg viewBox="0 0 24 24" aria-hidden="true">
                    <path
                      fill="currentColor"
                      d="M3.3 2.3 2 3.6l3.1 3.1C3 8.3 1.7 10.2 1.3 11c.4 1 3.7 8 10.7 8 2.1 0 3.9-.5 5.4-1.2l3 3 1.3-1.3L3.3 2.3zM12 17c-5.1 0-8-5.1-8.7-6.2.4-.7 1.4-2.2 3-3.4l2.1 2.1A4.9 4.9 0 0 0 12 17zm10.7-6.2c-.3.7-1.2 2.5-3 4L17.5 13A5 5 0 0 0 12 7c-.5 0-1 .1-1.5.2L8.3 5C9.5 4.4 10.7 4 12 4c7 0 10.3 7 10.7 6.8z"
                    />
                  </svg>
                )}
              </button>
            </div>
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
