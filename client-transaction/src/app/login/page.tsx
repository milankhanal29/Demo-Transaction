"use client";

import { useState } from "react";
import { useRouter } from "next/navigation";
import { useAuth } from "../context/AuthContext";
import toast from "react-hot-toast";

export default function LoginPage() {
  const { login } = useAuth();
  const router = useRouter();
  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");

 const handleLogin = async () => {
  try {
    const res = await fetch("https://localhost:7200/api/users/login", {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify({ email, password }),
    });

    const data = await res.json();
    console.log("Login response:", data);

    const token = data.accessToken || data.data?.accessToken;

    if (!res.ok || !token) {
      toast.error("Login Failed");

      return;
    }

    login(token);
    toast.success("Login Successfull")
    router.push("/");
    
  } catch (err) {
    console.error("Login error:", err);
    alert("Something went wrong during login");
  }
};


  return (
    <div className="p-6">
      <h1 className="text-xl mb-4">Login</h1>
      <input
        className="border p-2 mr-2"
        placeholder="Email"
        value={email}
        onChange={(e) => setEmail(e.target.value)}
      />
      <input
        className="border p-2 mr-2"
        type="password"
        placeholder="Password"
        value={password}
        onChange={(e) => setPassword(e.target.value)}
      />
      <button onClick={handleLogin} className="bg-blue-600 text-white px-4 py-2">
        Login
      </button>
    </div>
  );
}
