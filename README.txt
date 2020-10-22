1) For success application start you should be seed dbo.Asp.NetRoles
   - Just register a test account in the app
   Or
   Write whis to dbo.AspNetRoles
   Id					 Name	NormalizedName	ConcurrencyStamp
   f08f4fb2-3c77-4310-a981-17f04093d62d	 Admin  ADMIN		80e6abe9-a929-4853-b2af-0992ada00d2a

   Write whis ti dbo.AspNetUserRoles
   UserId		RoleId
   --Your user ID--	f08f4fb2-3c77-4310-a981-17f04093d62d

Application use Stripe Payment API, change SendGridKey and SendGridUser in OnlineMarket/appsettings.json

2) To send EMAIL, the application uses SendGrid. It's model is in OnlineMarket.Utility/EmailSender and EmailOptions
   To change API Key and Id go to OnlineMarket/appsettings.json

3) To send SMS, the application uses TWILIO. It's model is in in OnlineMarket.Utility/SmsSendSettings
   To change API PhoneNumber, AuthToken and Id go to OnlineMarket/appsettings.json
   To configure SMS sender go to Areas/Customer/Controller/CartController 
   PC: Turn on Permission to send SMS to your region! To change the first registration phone number for users, go to Identity / Pages / Account / Register and go to code line 38.
ПС:
Разрешение на отправку SMS не включено для региона, указанного в поле «Кому»: 7082362404
