"use client"
export default function Footer() {
  return (
    <footer className="bg-lime-700 text-white flex justify-between items-stretch">
      <div className="flex items-center p-4">
        <p className="text-sm">&copy; AI Jobs Portal. All rights reserved.</p>
      </div>
        <div className="flex items-center">
            <ul className="flex space-x-4 p-4">
                <li>
                    <a href="/privacy" className="hover:underline">Privacy Policy</a>
                </li>
                <li>
                    <a href="/terms" className="hover:underline">Terms of Service</a>
                </li>
                <li>
                    <a href="/contact" className="hover:underline">Contact</a>
                </li>
            </ul>
        </div>
    </footer>
    );
}