/**
 * 注文項目追加リクエストDTO
 */
@Data
public class AddOrderItemRequest {
    @NotBlank(message = "Product ID is required")
    private String productId;
    
    @NotNull(message = "Quantity is required")
    @Min(value = 1, message = "Quantity must be at least 1")
    private Integer quantity;
}