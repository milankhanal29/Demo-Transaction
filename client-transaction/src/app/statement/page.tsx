'use client';

import { useEffect, useState } from 'react';
import { useAuth } from '@/app/context/AuthContext';

interface Transaction {
  id: number;
  fromUserId: number;
  toUserId: number;
  amount: number;
  timestamp: string;
  senderName: string;
  senderAccount: string;
  receiverName: string;
  receiverAccount: string;
}

interface PagedResult<T> {
  currentPage: number;
  totalPages: number;
  pageSize: number;
  totalCount: number;
  items: T[];
}

export default function Statement() {
  const { token } = useAuth();
  const [transactions, setTransactions] = useState<Transaction[]>([]);
  const [userId, setUserId] = useState<string | null>(null);
  const [error, setError] = useState<string | null>(null);
  const [page, setPage] = useState(1);
  const pageSize = 5;
  const [totalPages, setTotalPages] = useState(1);

  const fetchTransactions = async (uid: string, pageNum: number) => {
    try {
      const res = await fetch(
        `https://localhost:7200/api/transactions/user/${uid}?pageNumber=${pageNum}&pageSize=${pageSize}`,
        {
          headers: { Authorization: `Bearer ${token}` },
        }
      );

      if (!res.ok) throw new Error('Failed to fetch transactions');
      const data: PagedResult<Transaction> = await res.json();
      setTransactions(data.items);
      setTotalPages(data.totalPages);
    } catch (err: any) {
      setError(err.message || 'Unknown error');
    }
  };

  useEffect(() => {
    if (!token) return;
    const decoded = JSON.parse(atob(token.split('.')[1]));
    const uid = decoded.userId;
    setUserId(uid);
    fetchTransactions(uid, page);
  }, [token, page]);

  const formatDate = (timestamp: string) => new Date(timestamp).toLocaleString();
  const isCredit = (tx: Transaction) => userId && tx.toUserId.toString() === userId;
  const isDebit = (tx: Transaction) => userId && tx.fromUserId.toString() === userId;

  return (
    <div className="max-w-5xl mx-auto mt-6 p-6 shadow rounded">
      <h2 className="text-3xl font-semibold mb-6 text-center">Transaction Statement</h2>

      {error && <p className="text-red-600 text-center">{error}</p>}

      {transactions.length === 0 ? (
        <p className="text-center text-gray-600">No transactions found.</p>
      ) : (
        <>
          <table className="w-full table-auto border shadow-sm rounded mb-4">
            <thead>
              <tr>
                <th className="px-4 py-2 border">#</th>
                <th className="px-4 py-2 border">Type</th>
                <th className="px-4 py-2 border">Sender</th>
                <th className="px-4 py-2 border">Receiver</th>
                <th className="px-4 py-2 border">Amount</th>
                <th className="px-4 py-2 border">Timestamp</th>
              </tr>
            </thead>
            <tbody>
              {transactions.map((tx, index) => (
                <tr key={tx.id} className={index % 2 === 0 ? 'bg-black' : 'bg-black-50'}>
                  <td className="px-4 py-2 border">{tx.id}</td>
                  <td className="px-4 py-2 border font-medium">
                    {isCredit(tx) ? (
                      <span className="text-green-600">Credit</span>
                    ) : (
                      <span className="text-red-600">Debit</span>
                    )}
                  </td>
                  <td className="px-4 py-2 border">
                    {tx.senderName}
                    <br />
                    <span className="text-sm text-gray-500">{tx.senderAccount}</span>
                  </td>
                  <td className="px-4 py-2 border">
                    {tx.receiverName}
                    <br />
                    <span className="text-sm text-gray-500">{tx.receiverAccount}</span>
                  </td>
                  <td
                    className={`px-4 py-2 border font-semibold ${
                      isCredit(tx) ? 'text-green-600' : 'text-red-600'
                    }`}
                  >
                    {isCredit(tx)
                      ? `+Rs. ${tx.amount.toFixed(2)}`
                      : `-Rs. ${tx.amount.toFixed(2)}`}
                  </td>
                  <td className="px-4 py-2 border">{formatDate(tx.timestamp)}</td>
                </tr>
              ))}
            </tbody>
          </table>

          <div className="flex justify-center items-center space-x-4">
            <button
              onClick={() => setPage((p) => Math.max(p - 1, 1))}
              disabled={page === 1}
              className="px-4 py-1 bg-black-200 rounded disabled:opacity-50"
            >
              Previous
            </button>
            <span>
              Page {page} of {totalPages}
            </span>
            <button
              onClick={() => setPage((p) => Math.min(p + 1, totalPages))}
              disabled={page === totalPages}
              className="px-4 py-1  rounded disabled:opacity-50"
            >
              Next
            </button>
          </div>
        </>
      )}
    </div>
  );
}
