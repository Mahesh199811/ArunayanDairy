import react from "@vitejs/plugin-react";
import { defineConfig } from "vite";

export default defineConfig({
  plugins: [react()],
  server: {
    hmr: {
      overlay: false,
    },
    watch: {
      ignored: ["**/.git/**", "**/node_modules/**"],
    },
  },
  preview: {
    port: 5173,
    strictPort: true,
  },
});
