import { Navigate, useLocation } from "react-router-dom";
import { useStore } from "../stores/store";

interface Props {
    children: JSX.Element;
}

export default function PrivateRoute({ children }: Props) {
    const { userStore: { isLoggedIn } } = useStore();
    const location = useLocation();

    return isLoggedIn ? children : <Navigate to="/" state={{ from: location }} replace />;
}
