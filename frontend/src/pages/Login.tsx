import { FormEvent, useState } from "react";
import { loginUser } from "../services/userService";

export default function Login() {
  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");

  async function handleSubmit(
    event: FormEvent
  ) {
    event.preventDefault();

    try {
      const result = await loginUser({
        email,
        password,
      });

      localStorage.setItem(
        "token",
        result.token
      );

      localStorage.setItem(
        "user",
        JSON.stringify(result.user)
      );

      alert("Login successful");
    } catch (error) {
      console.error(error);
      alert("Login failed");
    }
  }

  return (
    <div>
      <h1>Arunayan Dairy</h1>

      <h2>Login</h2>

      <form onSubmit={handleSubmit}>
        <div>
          <label>Email</label>

          <input
            type="email"
            value={email}
            onChange={(e) =>
              setEmail(e.target.value)
            }
          />
        </div>

        <div>
          <label>Password</label>

          <input
            type="password"
            value={password}
            onChange={(e) =>
              setPassword(e.target.value)
            }
          />
        </div>

        <button type="submit">
          Login
        </button>
      </form>
    </div>
  );
}
