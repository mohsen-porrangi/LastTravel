using BuildingBlocks.Contracts.Services;
using BuildingBlocks.Exceptions;

namespace UserManagement.API.Services
{
    public class UserManagementService(
     IUserRepository userRepository,
     ITokenService tokenService,
     IOtpService otpService,
     ILogger<UserManagementService> logger
     ) : IUserManagementService
    {
        public async Task<UserDetailDto> GetUserByIdAsync(Guid userId)
        {
            logger.LogInformation("Getting user by ID: {UserId}", userId);

            var user = await userRepository.GetByIdAsync(userId, default);
            if (user == null)
            {
                logger.LogWarning("User not found: {UserId}", userId);
                throw new NotFoundException("کاربر یافت نشد", $"کاربری با شناسه {userId} یافت نشد");
            }

            //TODO handle UserDetailDto
            return null; /*new UserDetailDto(
                user.Id,
                user.FirstName,
                user.LastFamily,
                user.MasterIdentity?.Mobile,
                user.IsActive
            );*/
        }
        public async Task<bool> UserExistsAsync(Guid userId)
        {
            var user = await userRepository.GetByIdAsync(userId, default);
            return user != null;
        }

        public async Task<bool> IsUserActiveAsync(Guid userId)
        {
            var user = await userRepository.GetByIdAsync(userId, default);
            if (user == null)
                throw new NotFoundException("کاربر یافت نشد", $"کاربری با شناسه {userId} یافت نشد");

            return user.MasterIdentity.IsActive;
        }
        public async Task<bool> ValidateCredentialsAsync(string mobile, string password)
        {
            if (string.IsNullOrEmpty(mobile) || string.IsNullOrEmpty(password))
                throw new BadRequestException("اطلاعات ورود ناقص است", "شماره موبایل و رمز عبور الزامی هستند");

            var identity = await userRepository.GetIdentityByMobileAsync(mobile);
            if (identity == null)
                return false;

            return BCrypt.Net.BCrypt.Verify(password, identity.PasswordHash);
        }
        public async Task<TokenResponseDto> AuthenticateAsync(AuthRequestDto request)
        {
            var identity = await userRepository.GetIdentityByMobileAsync(request.Mobile);
            if (identity == null)
            {
                logger.LogWarning("Authentication failed: Identity not found for mobile {Mobile}", request.Mobile);
                throw new UnauthorizedDomainException("اطلاعات ورود نامعتبر است");
            }

            bool isValid = false;

            if (!string.IsNullOrWhiteSpace(request.Password))
            {
                isValid = BCrypt.Net.BCrypt.Verify(request.Password, identity.PasswordHash);
                logger.LogInformation("Password validation for {Mobile}: {Result}", request.Mobile, isValid);
            }
            else if (!string.IsNullOrWhiteSpace(request.Otp))
            {
                isValid = await otpService.ValidateOtpAsync(request.Mobile, request.Otp);
                logger.LogInformation("OTP validation for {Mobile}: {Result}", request.Mobile, isValid);
            }
            else
            {
                throw new BadRequestException("اطلاعات ورود ناقص است", "باید رمز عبور یا کد یکبار مصرف ارائه شود");
            }

            if (!isValid)
            {
                logger.LogWarning("Authentication failed: Invalid credentials for {Mobile}", request.Mobile);
                throw new UnauthorizedDomainException("اطلاعات ورود نامعتبر است");
            }

            var user = await userRepository.GetUserByIdentityIdAsync(identity.Id);
            if (user == null)
            {
                logger.LogWarning("Authentication failed: User not found for identity {IdentityId}", identity.Id);
                throw new NotFoundException("کاربر یافت نشد", $"کاربری با شناسه هویت {identity.Id} یافت نشد");
            }

            if (!user.MasterIdentity.IsActive)
            {
                logger.LogWarning("Authentication failed: User account is inactive for {UserId}", user.Id);
                throw new ForbiddenDomainException("حساب کاربری غیرفعال است");
            }

            var permissions = await userRepository.GetUserPermissionsAsync(user.Id);
            string token = tokenService.GenerateToken(user, permissions);

            // Generate refresh token
            string refreshToken = Guid.NewGuid().ToString();
            await userRepository.StoreRefreshTokenAsync(identity.Id, refreshToken);

            logger.LogInformation("Authentication successful for user {UserId}", user.Id);

            return new TokenResponseDto(
                token,
                refreshToken,
                DateTime.UtcNow.AddHours(2)
            );
        }
        public Task<bool> ValidateTokenAsync(string token)
        {
            if (string.IsNullOrEmpty(token))
                throw new BadRequestException("توکن ارائه نشده است");

            try
            {
                logger.LogInformation("Validating token");
                return Task.FromResult(tokenService.ValidateToken(token));
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error validating token");
                throw new UnauthorizedDomainException("توکن نامعتبر است");
            }
        }
        public async Task<bool> HasPermissionAsync(Guid userId, string permissionCode)
        {
            if (string.IsNullOrEmpty(permissionCode))
                throw new BadRequestException("کد دسترسی ارائه نشده است");

            logger.LogInformation("Checking permission {Permission} for user {UserId}", permissionCode, userId);

            // ابتدا بررسی می‌کنیم که کاربر وجود داشته باشد
            var userExists = await UserExistsAsync(userId);
            if (!userExists)
                throw new NotFoundException("کاربر یافت نشد", $"کاربری با شناسه {userId} یافت نشد");

            var permissions = await userRepository.GetUserPermissionsAsync(userId);
            return permissions.Contains(permissionCode);
        }
    }
}
