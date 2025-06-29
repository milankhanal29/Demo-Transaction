'use client';
interface JwtPayload {
  role: string;
  email: string;
  sub?: string;
  [key: string]: any;
}
import { jwtDecode } from 'jwt-decode'
import {
  createContext,
  useContext,
  useState,
  useEffect,
  ReactNode,
} from 'react';

interface UserType {
  role: string;
  email: string;
}

interface AuthContextProps {
  token: string | null;
  user: UserType | null;
  login: (token: string) => void;
  logout: () => void;
  loading: boolean;
}

const AuthContext = createContext<AuthContextProps | undefined>(undefined);

export const AuthProvider = ({ children }: { children: ReactNode }) => {
  const [token, setToken] = useState<string | null>(null);
  const [user, setUser] = useState<UserType | null>(null);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    const storedToken = localStorage.getItem('accessToken');
    if (storedToken) {
      try {
        const decoded = jwtDecode<JwtPayload>(storedToken);
        console.log('Decoded token (init):', decoded); 
        setToken(storedToken);
        setUser({ role: decoded.role, email: decoded.email });
      } catch (err) {
        console.error('Failed to decode token:', err);
        localStorage.removeItem('accessToken');
      }
    }
    setLoading(false);
  }, []);

  const login = (newToken: string) => {
    localStorage.setItem('accessToken', newToken);
    setToken(newToken);

    try {
        const decoded = jwtDecode<JwtPayload>(newToken);
      console.log('Decoded token (login):', decoded);
      setUser({ role: decoded.role, email: decoded.email });
    } catch (e) {
      console.error('Token decode error on login:', e);
      logout();
    }
  };

  const logout = () => {
    localStorage.removeItem('accessToken');
    setToken(null);
    setUser(null);
  };

  return (
    <AuthContext.Provider value={{ token, user, login, logout, loading }}>
      {children}
    </AuthContext.Provider>
  );
};

export const useAuth = () => {
  const context = useContext(AuthContext);
  if (!context) throw new Error('useAuth must be used within AuthProvider');
  return context;
};
