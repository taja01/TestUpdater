Feature: User Login and Profile Management
  As a user of the application
  I want to log in and manage my profile
  So that I can access personalized features and update my details

Background:
	Given the application is running
	And the user is on the Login page

@TC13
Scenario: Successful Login with valid credentials
	Given the user provides valid login credentials
		| username | password |
		| johndoe  | pass123  |
	When the user clicks the "Login" button
	Then the user should see the Dashboard page
	And the user should see a welcome message "Welcome, John Doe!"

@TC14
Scenario: Login failure with invalid credentials
	Given the user provides invalid login credentials
		| username | password  |
		| johndoe  | wrongpass |
	When the user clicks the "Login" button
	Then the user should see an error message "Invalid username or password."
	And the user should remain on the Login page

@TC15
Scenario: Update profile details
	Given the user is logged in
	And the user is on the Profile page
	When the user updates their profile details with the following:
		| field         | value              |
		| Full Name     | Johnathan Doe      |
		| Email Address | john.doe@email.com |
		| Phone Number  | 123-456-7890       |
	And the user clicks the "Save" button
	Then the profile should be updated successfully
	And the user should see a confirmation message "Your profile has been updated."

@TC16
Scenario: Lockout after multiple failed login attempts
	Given the user attempts to log in with the wrong password 3 times
		| attempt | username | password   |
		| 1       | johndoe  | wrongpass1 |
		| 2       | johndoe  | wrongpass2 |
		| 3       | johndoe  | wrongpass3 |
	When the user attempts a 4th login with the wrong password
		| username | password   |
		| johndoe  | wrongpass4 |
	Then the user should see an error message "Your account is locked. Please contact support."
	And the user should not be able to log in