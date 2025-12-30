"""Comprehensive unit tests for auth.validate module."""

import pytest
from auth.validate import validate_user, is_valid_username, is_valid_email, ValidationError


class TestValidateUser:
    """Test cases for the validate_user function."""
    
    def test_valid_user_data(self):
        """Test with completely valid user data."""
        user_data = {
            'username': 'john_doe123',
            'email': 'john.doe@example.com',
            'password': 'SecurePass123!',
            'age': 25,
            'phone': '+1-555-123-4567'
        }
        result = validate_user(user_data)
        
        assert result['is_valid'] is True
        assert result['errors'] == []
        assert len(result['warnings']) == 0
    
    def test_minimal_valid_user_data(self):
        """Test with minimal required fields only."""
        user_data = {
            'username': 'alice',
            'email': 'alice@test.com',
            'password': 'Password1!'
        }
        result = validate_user(user_data)
        
        assert result['is_valid'] is True
        assert result['errors'] == []
    
    def test_none_user_data_raises_exception(self):
        """Test that None user_data raises ValidationError."""
        with pytest.raises(ValidationError, match="User data cannot be None"):
            validate_user(None)
    
    def test_non_dict_user_data_raises_exception(self):
        """Test that non-dictionary user_data raises ValidationError."""
        with pytest.raises(ValidationError, match="User data must be a dictionary"):
            validate_user("not a dict")
        
        with pytest.raises(ValidationError, match="User data must be a dictionary"):
            validate_user(123)
        
        with pytest.raises(ValidationError, match="User data must be a dictionary"):
            validate_user([])
    
    def test_missing_required_fields(self):
        """Test validation with missing required fields."""
        # Missing username
        user_data = {
            'email': 'test@example.com',
            'password': 'Password123!'
        }
        result = validate_user(user_data)
        assert result['is_valid'] is False
        assert 'Missing required field: username' in result['errors']
        
        # Missing email
        user_data = {
            'username': 'testuser',
            'password': 'Password123!'
        }
        result = validate_user(user_data)
        assert result['is_valid'] is False
        assert 'Missing required field: email' in result['errors']
        
        # Missing password
        user_data = {
            'username': 'testuser',
            'email': 'test@example.com'
        }
        result = validate_user(user_data)
        assert result['is_valid'] is False
        assert 'Missing required field: password' in result['errors']
    
    def test_none_required_fields(self):
        """Test validation with None values for required fields."""
        user_data = {
            'username': None,
            'email': 'test@example.com',
            'password': 'Password123!'
        }
        result = validate_user(user_data)
        assert result['is_valid'] is False
        assert 'Missing required field: username' in result['errors']
    
    def test_empty_required_fields(self):
        """Test validation with empty strings for required fields."""
        user_data = {
            'username': '',
            'email': 'test@example.com',
            'password': 'Password123!'
        }
        result = validate_user(user_data)
        assert result['is_valid'] is False
        assert "Field 'username' must be a non-empty string" in result['errors']
        
        user_data = {
            'username': '   ',  # Only whitespace
            'email': 'test@example.com',
            'password': 'Password123!'
        }
        result = validate_user(user_data)
        assert result['is_valid'] is False
        assert "Field 'username' must be a non-empty string" in result['errors']


class TestUsernameValidation:
    """Test cases for username validation."""
    
    def test_valid_usernames(self):
        """Test various valid username formats."""
        valid_usernames = [
            'user123',
            'john_doe',
            'alice-bob',
            'test_user_123',
            'a' * 50,  # Maximum length
            'abc'  # Minimum length
        ]
        
        for username in valid_usernames:
            user_data = {
                'username': username,
                'email': 'test@example.com',
                'password': 'Password123!'
            }
            result = validate_user(user_data)
            assert result['is_valid'] is True, f"Username '{username}' should be valid"
    
    def test_username_too_short(self):
        """Test username shorter than minimum length."""
        user_data = {
            'username': 'ab',
            'email': 'test@example.com',
            'password': 'Password123!'
        }
        result = validate_user(user_data)
        assert result['is_valid'] is False
        assert 'Username must be at least 3 characters long' in result['errors']
    
    def test_username_too_long(self):
        """Test username longer than maximum length."""
        user_data = {
            'username': 'a' * 51,
            'email': 'test@example.com',
            'password': 'Password123!'
        }
        result = validate_user(user_data)
        assert result['is_valid'] is False
        assert 'Username must be no more than 50 characters long' in result['errors']
    
    def test_username_invalid_characters(self):
        """Test username with invalid characters."""
        invalid_usernames = [
            'user@name',
            'user name',
            'user.name',
            'user#123',
            'user!',
            'user%test'
        ]
        
        for username in invalid_usernames:
            user_data = {
                'username': username,
                'email': 'test@example.com',
                'password': 'Password123!'
            }
            result = validate_user(user_data)
            assert result['is_valid'] is False
            assert 'Username can only contain letters, numbers, underscores, and hyphens' in result['errors']
    
    def test_username_starting_with_special_chars(self):
        """Test username starting with underscore or hyphen generates warning."""
        user_data = {
            'username': '_username',
            'email': 'test@example.com',
            'password': 'Password123!'
        }
        result = validate_user(user_data)
        assert result['is_valid'] is True
        assert 'Username should not start with underscore or hyphen' in result['warnings']
        
        user_data['username'] = '-username'
        result = validate_user(user_data)
        assert result['is_valid'] is True
        assert 'Username should not start with underscore or hyphen' in result['warnings']


class TestEmailValidation:
    """Test cases for email validation."""
    
    def test_valid_emails(self):
        """Test various valid email formats."""
        valid_emails = [
            'test@example.com',
            'user.name@domain.co.uk',
            'user+tag@example.org',
            'user123@test-domain.com',
            'a@b.co'
        ]
        
        for email in valid_emails:
            user_data = {
                'username': 'testuser',
                'email': email,
                'password': 'Password123!'
            }
            result = validate_user(user_data)
            assert result['is_valid'] is True, f"Email '{email}' should be valid"
    
    def test_invalid_emails(self):
        """Test various invalid email formats."""
        invalid_emails = [
            'invalid-email',
            '@example.com',
            'user@',
            'user@domain',
            'user..name@example.com',
            'user@domain..com',
            'user name@example.com',
            'user@domain.c'  # TLD too short
        ]
        
        for email in invalid_emails:
            user_data = {
                'username': 'testuser',
                'email': email,
                'password': 'Password123!'
            }
            result = validate_user(user_data)
            assert result['is_valid'] is False, f"Email '{email}' should be invalid"
            assert 'Invalid email format' in result['errors']
    
    def test_email_too_long(self):
        """Test email longer than maximum length."""
        long_email = 'a' * 250 + '@example.com'  # Over 254 characters
        user_data = {
            'username': 'testuser',
            'email': long_email,
            'password': 'Password123!'
        }
        result = validate_user(user_data)
        assert result['is_valid'] is False
        assert 'Email address is too long' in result['errors']


class TestPasswordValidation:
    """Test cases for password validation."""
    
    def test_valid_strong_passwords(self):
        """Test various strong password formats."""
        strong_passwords = [
            'Password123!',
            'MySecure@Pass1',
            'Complex#Password2024',
            'StrongP@ss123'
        ]
        
        for password in strong_passwords:
            user_data = {
                'username': 'testuser',
                'email': 'test@example.com',
                'password': password
            }
            result = validate_user(user_data)
            assert result['is_valid'] is True, f"Password '{password}' should be valid"
    
    def test_password_too_short(self):
        """Test password shorter than minimum length."""
        user_data = {
            'username': 'testuser',
            'email': 'test@example.com',
            'password': 'Pass1!'
        }
        result = validate_user(user_data)
        assert result['is_valid'] is False
        assert 'Password must be at least 8 characters long' in result['errors']
    
    def test_password_too_long(self):
        """Test password longer than maximum length."""
        user_data = {
            'username': 'testuser',
            'email': 'test@example.com',
            'password': 'a' * 129
        }
        result = validate_user(user_data)
        assert result['is_valid'] is False
        assert 'Password must be no more than 128 characters long' in result['errors']
    
    def test_weak_passwords(self):
        """Test passwords that don't meet strength requirements."""
        weak_passwords = [
            'STRONGONE',  # Only uppercase, no lowercase, no digits, no special chars
            'weakpass',  # Only lowercase
            'WEAKPASS',  # Only uppercase
            '12345678',  # Only digits (but this is common, so test separately)
        ]
        
        for password in weak_passwords[:-1]:  # Skip the last one for now
            user_data = {
                'username': 'testuser',
                'email': 'test@example.com',
                'password': password
            }
            result = validate_user(user_data)
            assert result['is_valid'] is False
            assert 'Password must contain at least 2 of: uppercase letter, lowercase letter, digit, special character' in result['errors']
        
        # Test the common password separately
        user_data = {
            'username': 'testuser',
            'email': 'test@example.com',
            'password': '12345678'
        }
        result = validate_user(user_data)
        assert result['is_valid'] is False
        assert 'Password is too common and easily guessable' in result['errors']
    
    def test_moderately_strong_passwords_with_warnings(self):
        """Test passwords that are valid but generate warnings."""
        moderate_passwords = [
            'password1',  # Only 2 types: lower, digit
            'password!',  # Only 2 types: lower, special
            'PASSWORD1',  # Only 2 types: upper, digit
        ]
        
        for password in moderate_passwords:
            user_data = {
                'username': 'testuser',
                'email': 'test@example.com',
                'password': password
            }
            result = validate_user(user_data)
            assert result['is_valid'] is True
            assert 'Consider using a stronger password with more character types' in result['warnings']
    
    def test_common_passwords(self):
        """Test common passwords that should be rejected."""
        common_passwords = [
            'password',
            '12345678',
            'qwerty123',
            'admin123'
        ]
        
        for password in common_passwords:
            user_data = {
                'username': 'testuser',
                'email': 'test@example.com',
                'password': password
            }
            result = validate_user(user_data)
            assert result['is_valid'] is False
            assert 'Password is too common and easily guessable' in result['errors']
    
    def test_password_contains_username_warning(self):
        """Test password containing username generates warning."""
        user_data = {
            'username': 'johndoe',
            'email': 'test@example.com',
            'password': 'JohnDoe123!'
        }
        result = validate_user(user_data)
        assert result['is_valid'] is True
        assert 'Password should not contain the username' in result['warnings']


class TestAgeValidation:
    """Test cases for optional age validation."""
    
    def test_valid_ages(self):
        """Test various valid ages."""
        valid_ages = [13, 18, 25, 65, 100]
        
        for age in valid_ages:
            user_data = {
                'username': 'testuser',
                'email': 'test@example.com',
                'password': 'Password123!',
                'age': age
            }
            result = validate_user(user_data)
            assert result['is_valid'] is True
    
    def test_minor_age_warning(self):
        """Test that users under 18 get a warning."""
        user_data = {
            'username': 'testuser',
            'email': 'test@example.com',
            'password': 'Password123!',
            'age': 16
        }
        result = validate_user(user_data)
        assert result['is_valid'] is True
        assert 'User is under 18 years old' in result['warnings']
    
    def test_age_too_young(self):
        """Test age under minimum."""
        user_data = {
            'username': 'testuser',
            'email': 'test@example.com',
            'password': 'Password123!',
            'age': 12
        }
        result = validate_user(user_data)
        assert result['is_valid'] is False
        assert 'User must be at least 13 years old' in result['errors']
    
    def test_age_too_old(self):
        """Test unrealistic age."""
        user_data = {
            'username': 'testuser',
            'email': 'test@example.com',
            'password': 'Password123!',
            'age': 200
        }
        result = validate_user(user_data)
        assert result['is_valid'] is False
        assert 'Age must be realistic (under 150)' in result['errors']
    
    def test_age_not_integer(self):
        """Test non-integer age values."""
        invalid_ages = ['25', 25.5, None]
        
        for age in invalid_ages:
            user_data = {
                'username': 'testuser',
                'email': 'test@example.com',
                'password': 'Password123!',
                'age': age
            }
            result = validate_user(user_data)
            if age is not None:  # None is allowed for optional field
                assert result['is_valid'] is False
                assert 'Age must be an integer' in result['errors']


class TestPhoneValidation:
    """Test cases for optional phone validation."""
    
    def test_valid_phone_numbers(self):
        """Test various valid phone number formats."""
        valid_phones = [
            '+1234567890',
            '+1-555-123-4567',
            '(555) 123-4567',
            '555.123.4567',
            '+44 20 7946 0958',
            '+33123456789'
        ]
        
        for phone in valid_phones:
            user_data = {
                'username': 'testuser',
                'email': 'test@example.com',
                'password': 'Password123!',
                'phone': phone
            }
            result = validate_user(user_data)
            assert result['is_valid'] is True, f"Phone '{phone}' should be valid"
    
    def test_invalid_phone_numbers(self):
        """Test various invalid phone number formats."""
        invalid_phones = [
            '123',  # Too short
            '+0123456789',  # Starts with 0 after country code
            'abc-def-ghij',  # Contains letters
            '+',  # Just a plus sign
            '123456789012345678'  # Too long
        ]
        
        for phone in invalid_phones:
            user_data = {
                'username': 'testuser',
                'email': 'test@example.com',
                'password': 'Password123!',
                'phone': phone
            }
            result = validate_user(user_data)
            assert result['is_valid'] is False, f"Phone '{phone}' should be invalid"
            assert 'Invalid phone number format' in result['errors']
    
    def test_empty_phone_allowed(self):
        """Test that empty phone string is allowed."""
        user_data = {
            'username': 'testuser',
            'email': 'test@example.com',
            'password': 'Password123!',
            'phone': ''
        }
        result = validate_user(user_data)
        assert result['is_valid'] is True


class TestHelperFunctions:
    """Test cases for helper functions."""
    
    def test_is_valid_username(self):
        """Test is_valid_username helper function."""
        assert is_valid_username('validuser') is True
        assert is_valid_username('user_123') is True
        assert is_valid_username('user-name') is True
        
        assert is_valid_username('ab') is False  # Too short
        assert is_valid_username('a' * 51) is False  # Too long
        assert is_valid_username('user@name') is False  # Invalid chars
        assert is_valid_username('') is False  # Empty
        assert is_valid_username(None) is False  # None
        assert is_valid_username(123) is False  # Not string
    
    def test_is_valid_email(self):
        """Test is_valid_email helper function."""
        assert is_valid_email('test@example.com') is True
        assert is_valid_email('user.name@domain.co.uk') is True
        
        assert is_valid_email('invalid-email') is False
        assert is_valid_email('@example.com') is False
        assert is_valid_email('user@') is False
        assert is_valid_email('') is False  # Empty
        assert is_valid_email(None) is False  # None
        assert is_valid_email(123) is False  # Not string
        assert is_valid_email('a' * 250 + '@example.com') is False  # Too long


class TestComplexScenarios:
    """Test cases for complex validation scenarios."""
    
    def test_multiple_errors_and_warnings(self):
        """Test user data with multiple validation issues."""
        user_data = {
            'username': 'ab',  # Too short
            'email': 'invalid-email',  # Invalid format
            'password': 'weak',  # Too short and weak
            'age': 12,  # Too young
            'phone': '123'  # Invalid format
        }
        result = validate_user(user_data)
        
        assert result['is_valid'] is False
        assert len(result['errors']) >= 4  # Multiple errors
        assert 'Username must be at least 3 characters long' in result['errors']
        assert 'Invalid email format' in result['errors']
        assert 'Password must be at least 8 characters long' in result['errors']
        assert 'User must be at least 13 years old' in result['errors']
        assert 'Invalid phone number format' in result['errors']
    
    def test_edge_case_boundary_values(self):
        """Test boundary values for all validations."""
        # Test minimum valid values
        user_data = {
            'username': 'abc',  # Exactly 3 chars
            'email': 'a@b.co',  # Minimal valid email
            'password': 'Pass123!',  # Exactly 8 chars with requirements
            'age': 13,  # Minimum age
            'phone': '+1234567890'  # Minimal valid phone
        }
        result = validate_user(user_data)
        assert result['is_valid'] is True
        
        # Test maximum valid values
        user_data = {
            'username': 'a' * 50,  # Exactly 50 chars
            'email': 'test@example.com',
            'password': 'P@ss1' + 'a' * 123,  # Exactly 128 chars
            'age': 149,  # Just under limit
            'phone': '+12345678901234'  # 14 digits after country code
        }
        result = validate_user(user_data)
        assert result['is_valid'] is True