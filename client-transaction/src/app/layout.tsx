import './globals.css';
import Navbar from '@/app/components/Navbar'; 
import { Providers } from './Providers';
import { ToasterProvider } from './ToastrProvider';
export const metadata = {
  title: 'Demo Transaction App',
  description: 'App with .NET backend',
};

export default function RootLayout({ children }: { children: React.ReactNode }) {
  return (
    <html lang="en">
      <body>
        <Providers>
          <Navbar />
          {children}
          <ToasterProvider/>
        </Providers>
      </body>
    </html>
  );
}