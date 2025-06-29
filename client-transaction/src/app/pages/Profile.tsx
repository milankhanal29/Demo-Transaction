"use client";

import { useEffect, useState } from "react";
import { useAuth } from "../context/AuthContext";
import { fetchWithToken } from "../utils/fetchWithToken";

export default function ProfilePage() {
  const { token } = useAuth();
  const [profile, setProfile] = useState<any>(null);

  useEffect(() => {
    if (token) {
      fetchWithToken("https://localhost:7200/api/users/profile", token)
        .then((res) => res.json())
        .then(setProfile)
        .catch(console.error);
    }
  }, [token]);

  if (!profile) return <div>Loading...</div>;

  return (
    <div className="p-6">
      <h1 className="text-xl">Profile</h1>
      <p>Email: {profile.email}</p>
      <p>Role: {profile.role}</p>
    </div>
  );
}