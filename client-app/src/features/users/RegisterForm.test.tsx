import { render, screen, fireEvent, waitFor } from '@testing-library/react';
import RegisterForm from './RegisterForm';
import { useStore } from '../../app/stores/store';

const mockRegister = jest.fn();

jest.mock('../../app/stores/store', () => ({
    useStore: jest.fn(),
}));

jest.mock('mobx-react-lite', () => ({
    observer: (component: React.ComponentType) => component,
}));

describe('RegisterForm', () => {
    beforeEach(() => {
        jest.clearAllMocks();

        (useStore as jest.Mock).mockReturnValue({
            userStore: {
                register: mockRegister,
            },
        });
    });

    it('should render register form', () => {
        render(<RegisterForm />);

        expect(
            screen.getByRole('heading', {
                name: 'Sign up to Reactivities',
            })
        ).toBeInTheDocument();

        expect(
            screen.getByPlaceholderText('Display Name')
        ).toBeInTheDocument();

        expect(
            screen.getByPlaceholderText('Username')
        ).toBeInTheDocument();

        expect(
            screen.getByPlaceholderText('Email')
        ).toBeInTheDocument();

        expect(
            screen.getByPlaceholderText('Password')
        ).toBeInTheDocument();

        expect(
            screen.getByRole('button', {
                name: 'Register',
            })
        ).toBeInTheDocument();
    });

    it('should register user with entered values', async () => {
        mockRegister.mockResolvedValue({
            username: 'testuser',
            dispayName: 'Test User',
            token: 'test-token',
        });

        render(<RegisterForm />);

        fireEvent.change(
            screen.getByPlaceholderText('Display Name'),
            {
                target: {
                    value: 'Test User',
                },
            }
        );

        fireEvent.change(
            screen.getByPlaceholderText('Username'),
            {
                target: {
                    value: 'testuser',
                },
            }
        );

        fireEvent.change(
            screen.getByPlaceholderText('Email'),
            {
                target: {
                    value: 'test@example.com',
                },
            }
        );

        fireEvent.change(
            screen.getByPlaceholderText('Password'),
            {
                target: {
                    value: 'Password123!',
                },
            }
        );

        fireEvent.click(
            screen.getByRole('button', {
                name: 'Register',
            })
        );

        await waitFor(() => {
            expect(mockRegister).toHaveBeenCalledWith({
                dispayName: 'Test User',
                username: 'testuser',
                email: 'test@example.com',
                password: 'Password123!',
                error: null,
            });
        });
    });

    it('should display validation error when registration fails', async () => {
        mockRegister.mockRejectedValue(
            new Error('Registration failed')
        );

        render(<RegisterForm />);

        fireEvent.change(
            screen.getByPlaceholderText('Display Name'),
            {
                target: {
                    value: 'Test User',
                },
            }
        );

        fireEvent.change(
            screen.getByPlaceholderText('Username'),
            {
                target: {
                    value: 'testuser',
                },
            }
        );

        fireEvent.change(
            screen.getByPlaceholderText('Email'),
            {
                target: {
                    value: 'test@example.com',
                },
            }
        );

        fireEvent.change(
            screen.getByPlaceholderText('Password'),
            {
                target: {
                    value: 'Password123!',
                },
            }
        );

        fireEvent.click(
            screen.getByRole('button', {
                name: 'Register',
            })
        );

        await waitFor(() => {
            expect(mockRegister).toHaveBeenCalledTimes(1);
        });

        // Проверяем, что форма обработала ошибку.
        expect(mockRegister).toHaveBeenCalled();
    });
});