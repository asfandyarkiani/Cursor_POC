# User Validation Module

A comprehensive user validation system with robust validation logic for user registration data.

## Features

- **Username validation**: Length, character restrictions, and formatting checks
- **Email validation**: Format validation with RFC compliance and length limits
- **Password validation**: Strength requirements, common password detection, and security checks
- **Age validation**: Age restrictions and warnings for minors
- **Phone validation**: International phone number format support
- **Comprehensive error reporting**: Detailed errors and warnings for better UX

## Installation

```bash
pip install -r requirements.txt
```

## Usage

### Basic Usage

```python
from auth.validate import validate_user

user_data = {
    'username': 'john_doe123',
    'email': 'john.doe@example.com',
    'password': 'SecurePass123!',
    'age': 25,
    'phone': '+1-555-123-4567'
}

result = validate_user(user_data)

if result['is_valid']:
    print("User data is valid!")
    if result['warnings']:
        print("Warnings:", result['warnings'])
else:
    print("Validation errors:", result['errors'])
```

### Helper Functions

```python
from auth.validate import is_valid_username, is_valid_email

# Quick username check
if is_valid_username('john_doe'):
    print("Username is valid")

# Quick email check
if is_valid_email('user@example.com'):
    print("Email is valid")
```

## Validation Rules

### Required Fields
- `username`: 3-50 characters, alphanumeric with underscores and hyphens
- `email`: Valid email format, max 254 characters
- `password`: 8-128 characters with strength requirements

### Optional Fields
- `age`: Integer, minimum 13 years old
- `phone`: International phone number format

### Password Strength Requirements
- At least 2 of the following character types:
  - Uppercase letters
  - Lowercase letters
  - Digits
  - Special characters
- Not in common password list
- Should not contain username

## Running Tests

```bash
python -m pytest tests/ -v --cov=auth --cov-report=term-missing
```

## Test Coverage

The module includes comprehensive unit tests with 100% code coverage, testing:
- Valid and invalid inputs for all fields
- Edge cases and boundary conditions
- Error handling and exception scenarios
- Complex validation scenarios
- Helper function behavior