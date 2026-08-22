import { render, screen } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import LoginForm from './LoginForm';
import { useStore } from '../../app/stores/store';

jest.mock('../../app/stores/store', () => ({
    useStore: jest.fn(),
}));

const mockLogin = jest.fn();

describe('LoginForm', () => {
    beforeEach(() => {
        jest.clearAllMocks();

        (useStore as jest.Mock).mockReturnValue({
            userStore: {
                login: mockLogin,
            },
        });
    });

    it('should render login form', () => {
        render(<LoginForm />);

        expect(
            screen.getByRole('heading', {
                name: 'Login to Reactivities',
            })
        ).toBeInTheDocument();

        expect(
            screen.getByPlaceholderText('Email')
        ).toBeInTheDocument();

        expect(
            screen.getByPlaceholderText('Password')
        ).toBeInTheDocument();

        expect(
            screen.getByRole('button', {
                name: 'Login',
            })
        ).toBeInTheDocument();
    });

    it('should call login with entered credentials', async () => {
        const user = userEvent.setup();

        mockLogin.mockResolvedValue(undefined);

        render(<LoginForm />);

        await user.type(
            screen.getByPlaceholderText('Email'),
            'test@example.com'
        );

        await user.type(
            screen.getByPlaceholderText('Password'),
            'Password123!'
        );

        await user.click(
            screen.getByRole('button', {
                name: 'Login',
            })
        );

        expect(mockLogin).toHaveBeenCalledTimes(1);

        expect(mockLogin).toHaveBeenCalledWith({
            email: 'test@example.com',
            password: 'Password123!',
            error: null,
        });
    });

    it('should display error when login fails', async () => {
        const user = userEvent.setup();

        mockLogin.mockRejectedValue(
            new Error('Invalid credentials')
        );

        render(<LoginForm />);

        await user.type(
            screen.getByPlaceholderText('Email'),
            'test@example.com'
        );

        await user.type(
            screen.getByPlaceholderText('Password'),
            'wrong-password'
        );

        await user.click(
            screen.getByRole('button', {
                name: 'Login',
            })
        );

        expect(
            await screen.findByText('Invalidemail or password')
        ).toBeInTheDocument();
    });
});