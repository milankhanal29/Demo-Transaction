'use client';

import Link from "next/link";
import { useAuth } from "../context/AuthContext";
import { useRouter } from "next/navigation";

export default function Navbar() {
  const { user, logout, loading } = useAuth();
    const router = useRouter();

  const handleLogout = () => {
    logout();        
    router.push("/login");  };
  if (loading) return null; 

  return (
    <nav className="bg-gray-800 text-white p-4 flex justify-between">
      <div className="font-bold text-xl">Mk</div>
      <div className="space-x-4">
        {!user ? (
          <>
            <Link href="/login">Login</Link>
            <Link href="/register">Register</Link>
          </>
        ) : (
          <>
            <Link href="/">Home</Link>

            {user.role === "Admin" && <Link href="/users">User List</Link>}
            {user.role === "Merchant" && (
              <>
                <Link href="/transfer">Transfer Amount</Link>
                <Link href="/transfer-bulk">Transfer Bulk Amount</Link>
              </>
            )}
            {(user.role === "User" || user.role === "Merchant") && <Link href="/statement">View Statements</Link>}
            {(user.role === "User") && <Link href="/send-money">Send Money</Link>}

            {(user.role === "Merchant" || user.role === "User") && (
              <Link href="/profile">Profile</Link>
            )}

            <button onClick={handleLogout} className="text-red-300">Logout</button>
          </>
        )}
      </div>
    </nav>
  );
}
