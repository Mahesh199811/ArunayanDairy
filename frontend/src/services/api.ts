import axios from "axios";

export const userApi = axios.create({
  baseURL: "http://localhost:5051",
});

export const productApi = axios.create({
  baseURL: "http://localhost:5296",
});

export const orderApi = axios.create({
  baseURL: "http://localhost:5275",
});
