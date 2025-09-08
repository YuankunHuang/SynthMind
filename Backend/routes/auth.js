const express = require('express');
const bcrypt = require('bcrypt');
const jwt = require('jsonwebtoken');
const { body, validationResult } = require('express-validator');
const { query } = require('../config/database');
const { authenticateToken } = require('../middleware/auth');

const router = express.Router();

// Validation rules
const loginValidation = [
    body('username')
        .isLength({ min: 3, max: 50 })
        .withMessage('Username must be between 3 and 50 characters')
        .matches(/^[a-zA-Z0-9_]+$/)
        .withMessage('Username can only contain letters, numbers, and underscores'),
    body('password')
        .isLength({ min: 6, max: 100 })
        .withMessage('Password must be between 6 and 100 characters')
];

const registerValidation = [
    ...loginValidation,
    body('email')
        .optional()
        .isEmail()
        .withMessage('Must be a valid email address'),
    body('nickname')
        .optional()
        .isLength({ max: 100 })
        .withMessage('Nickname must be less than 100 characters')
];

// Helper functions
const generateToken = (user) => {
    return jwt.sign(
        { 
            userId: user.id, 
            username: user.username,
            avatar: user.avatar,
            nickname: user.nickname
        },
        process.env.JWT_SECRET,
        { expiresIn: process.env.JWT_EXPIRES_IN || '7d' }
    );
};

const hashPassword = async (password) => {
    const saltRounds = parseInt(process.env.BCRYPT_ROUNDS) || 12;
    return await bcrypt.hash(password, saltRounds);
};

// Routes

/**
 * POST /api/auth/login
 * Login user with username and password
 */
router.post('/login', loginValidation, async (req, res) => {
    try {
        // Check validation errors
        const errors = validationResult(req);
        if (!errors.isEmpty()) {
            return res.status(400).json({
                success: false,
                message: 'Validation failed',
                errors: errors.array()
            });
        }

        const { username, password } = req.body;

        // Find user by username
        const userResult = await query(
            'SELECT id, username, password_hash, avatar, nickname, is_active FROM users WHERE username = $1',
            [username]
        );

        if (userResult.rows.length === 0) {
            return res.status(401).json({
                success: false,
                message: 'Invalid credentials'
            });
        }

        const user = userResult.rows[0];

        // Check if user is active
        if (!user.is_active) {
            return res.status(401).json({
                success: false,
                message: 'Account is disabled'
            });
        }

        // Verify password
        const isPasswordValid = await bcrypt.compare(password, user.password_hash);
        if (!isPasswordValid) {
            return res.status(401).json({
                success: false,
                message: 'Invalid credentials'
            });
        }

        // Update last login
        await query(
            'UPDATE users SET last_login = CURRENT_TIMESTAMP WHERE id = $1',
            [user.id]
        );

        // Generate JWT token
        const token = generateToken(user);

        // Log successful login
        console.log(`✅ User ${username} logged in successfully`);

        res.json({
            success: true,
            message: 'Login successful',
            token,
            user: {
                id: user.id,
                username: user.username,
                avatar: user.avatar,
                nickname: user.nickname
            }
        });

    } catch (error) {
        console.error('❌ Login error:', error);
        res.status(500).json({
            success: false,
            message: 'Internal server error'
        });
    }
});

/**
 * POST /api/auth/register
 * Register a new user
 */
router.post('/register', registerValidation, async (req, res) => {
    try {
        // Check validation errors
        const errors = validationResult(req);
        if (!errors.isEmpty()) {
            return res.status(400).json({
                success: false,
                message: 'Validation failed',
                errors: errors.array()
            });
        }

        const { username, password, email, nickname } = req.body;

        // Check if username already exists
        const existingUser = await query(
            'SELECT id FROM users WHERE username = $1',
            [username]
        );

        if (existingUser.rows.length > 0) {
            return res.status(409).json({
                success: false,
                message: 'Username already exists'
            });
        }

        // Check if email already exists (if provided)
        if (email) {
            const existingEmail = await query(
                'SELECT id FROM users WHERE email = $1',
                [email]
            );

            if (existingEmail.rows.length > 0) {
                return res.status(409).json({
                    success: false,
                    message: 'Email already exists'
                });
            }
        }

        // Hash password
        const passwordHash = await hashPassword(password);

        // Create user
        const newUser = await query(
            `INSERT INTO users (username, password_hash, email, nickname) 
             VALUES ($1, $2, $3, $4) 
             RETURNING id, username, avatar, nickname`,
            [username, passwordHash, email || null, nickname || username]
        );

        const user = newUser.rows[0];

        // Generate JWT token
        const token = generateToken(user);

        console.log(`✅ New user ${username} registered successfully`);

        res.status(201).json({
            success: true,
            message: 'Registration successful',
            token,
            user: {
                id: user.id,
                username: user.username,
                avatar: user.avatar,
                nickname: user.nickname
            }
        });

    } catch (error) {
        console.error('❌ Registration error:', error);
        res.status(500).json({
            success: false,
            message: 'Internal server error'
        });
    }
});

/**
 * POST /api/auth/verify
 * Verify JWT token validity
 */
router.post('/verify', authenticateToken, async (req, res) => {
    try {
        // If middleware passed, token is valid
        const userId = req.user.userId;

        // Optionally fetch fresh user data
        const userResult = await query(
            'SELECT id, username, avatar, nickname, is_active FROM users WHERE id = $1',
            [userId]
        );

        if (userResult.rows.length === 0 || !userResult.rows[0].is_active) {
            return res.status(401).json({
                success: false,
                message: 'User not found or inactive'
            });
        }

        const user = userResult.rows[0];

        res.json({
            success: true,
            message: 'Token is valid',
            user: {
                id: user.id,
                username: user.username,
                avatar: user.avatar,
                nickname: user.nickname
            }
        });

    } catch (error) {
        console.error('❌ Token verification error:', error);
        res.status(500).json({
            success: false,
            message: 'Internal server error'
        });
    }
});

/**
 * POST /api/auth/logout
 * Logout user (optionally revoke token)
 */
router.post('/logout', authenticateToken, async (req, res) => {
    try {
        // In a more complex system, you might want to blacklist the token
        // For now, we'll just return success since JWT is stateless
        
        console.log(`✅ User ${req.user.username} logged out`);
        
        res.json({
            success: true,
            message: 'Logout successful'
        });

    } catch (error) {
        console.error('❌ Logout error:', error);
        res.status(500).json({
            success: false,
            message: 'Internal server error'
        });
    }
});

module.exports = router;