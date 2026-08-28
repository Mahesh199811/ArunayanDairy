export interface StoredUser {
  id: string;
  fullName: string;
  email: string;
}

export function readStoredUser(): StoredUser | null {
  try {
    const raw = localStorage.getItem("user");
    if (!raw) {
      return null;
    }

    const parsed = JSON.parse(raw) as StoredUser;
    if (!parsed || typeof parsed.id !== "string") {
      return null;
    }

    return {
      id: parsed.id,
      fullName: typeof parsed.fullName === "string" ? parsed.fullName : "",
      email: typeof parsed.email === "string" ? parsed.email : "",
    };
  } catch {
    return null;
  }
}

export function readStoredName() {
  return readStoredUser()?.fullName ?? "";
}
