"""User validation module with comprehensive validation logic."""

import re
from typing import Dict, List, Optional, Union


class ValidationError(Exception):
    """Custom exception for validation errors."""
    pass


def validate_user(user_data: Dict[str, Union[str, int, None]]) -> Dict[str, Union[bool, List[str]]]:
    """
    Validate user data with comprehensive checks.
    
    Args:
        user_data (Dict): Dictionary containing user information with keys:
            - username (str): Username to validate
            - email (str): Email address to validate  
            - password (str): Password to validate
            - age (int, optional): User's age
            - phone (str, optional): Phone number
    
    Returns:
        Dict: Validation result with structure:
            {
                'is_valid': bool,
                'errors': List[str],
                'warnings': List[str]
            }
    
    Raises:
        ValidationError: If user_data is None or not a dictionary
    """
    if user_data is None:
        raise ValidationError("User data cannot be None")
    
    if not isinstance(user_data, dict):
        raise ValidationError("User data must be a dictionary")
    
    errors = []
    warnings = []
    
    # Validate required fields
    required_fields = ['username', 'email', 'password']
    for field in required_fields:
        if field not in user_data or user_data[field] is None:
            errors.append(f"Missing required field: {field}")
        elif not isinstance(user_data[field], str) or not user_data[field].strip():
            errors.append(f"Field '{field}' must be a non-empty string")
    
    # If required fields are missing, return early
    if errors:
        return {
            'is_valid': False,
            'errors': errors,
            'warnings': warnings
        }
    
    # Validate username
    username = user_data['username'].strip()
    if len(username) < 3:
        errors.append("Username must be at least 3 characters long")
    elif len(username) > 50:
        errors.append("Username must be no more than 50 characters long")
    elif not re.match(r'^[a-zA-Z0-9_-]+$', username):
        errors.append("Username can only contain letters, numbers, underscores, and hyphens")
    elif username.startswith('_') or username.startswith('-'):
        warnings.append("Username should not start with underscore or hyphen")
    
    # Validate email
    email = user_data['email'].strip().lower()
    email_pattern = r'^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$'
    # Check for consecutive dots which are invalid
    if '..' in email or not re.match(email_pattern, email):
        errors.append("Invalid email format")
    elif len(email) > 254:
        errors.append("Email address is too long")
    
    # Validate password
    password = user_data['password']
    if len(password) < 8:
        errors.append("Password must be at least 8 characters long")
    elif len(password) > 128:
        errors.append("Password must be no more than 128 characters long")
    else:
        # Check password strength
        has_upper = bool(re.search(r'[A-Z]', password))
        has_lower = bool(re.search(r'[a-z]', password))
        has_digit = bool(re.search(r'\d', password))
        has_special = bool(re.search(r'[!@#$%^&*(),.?":{}|<>]', password))
        
        strength_score = sum([has_upper, has_lower, has_digit, has_special])
        
        if strength_score < 2:
            errors.append("Password must contain at least 2 of: uppercase letter, lowercase letter, digit, special character")
        elif strength_score == 2:
            warnings.append("Consider using a stronger password with more character types")
        
        # Check for common weak patterns first
        if password.lower() in ['password', '12345678', 'qwerty123', 'admin123']:
            errors.append("Password is too common and easily guessable")
        else:
            # Only check username in password if it's not a common password
            if username.lower() in password.lower():
                warnings.append("Password should not contain the username")
    
    # Validate optional age field
    if 'age' in user_data and user_data['age'] is not None:
        age = user_data['age']
        if not isinstance(age, int):
            errors.append("Age must be an integer")
        elif age < 13:
            errors.append("User must be at least 13 years old")
        elif age > 150:
            errors.append("Age must be realistic (under 150)")
        elif age < 18:
            warnings.append("User is under 18 years old")
    
    # Validate optional phone field
    if 'phone' in user_data and user_data['phone'] is not None:
        phone = user_data['phone'].strip()
        if phone:  # Only validate if phone is provided and not empty
            # Remove common formatting characters
            clean_phone = re.sub(r'[^\d+]', '', phone)
            if not re.match(r'^\+?[1-9]\d{7,14}$', clean_phone):
                errors.append("Invalid phone number format")
    
    return {
        'is_valid': len(errors) == 0,
        'errors': errors,
        'warnings': warnings
    }


def is_valid_username(username: str) -> bool:
    """
    Quick validation check for username only.
    
    Args:
        username (str): Username to validate
        
    Returns:
        bool: True if username is valid, False otherwise
    """
    if not isinstance(username, str) or not username.strip():
        return False
    
    username = username.strip()
    return (
        3 <= len(username) <= 50 and
        re.match(r'^[a-zA-Z0-9_-]+$', username) is not None
    )


def is_valid_email(email: str) -> bool:
    """
    Quick validation check for email only.
    
    Args:
        email (str): Email to validate
        
    Returns:
        bool: True if email is valid, False otherwise
    """
    if not isinstance(email, str) or not email.strip():
        return False
    
    email = email.strip().lower()
    email_pattern = r'^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$'
    return len(email) <= 254 and '..' not in email and re.match(email_pattern, email) is not None